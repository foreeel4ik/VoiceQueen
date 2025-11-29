using System;
using NAudio.Wave;

namespace VoiceQueen
{
    public class AudioEngine : IDisposable
    {
        private readonly EffectsProcessor _effectsProcessor;
        private readonly EffectParameterSet _parameters;
        private float[][] _inputBuffers = Array.Empty<float[]>();
        private float[][] _pitchBuffers = Array.Empty<float[]>();
        private WaveInEvent? _capture;
        private WaveOutEvent? _playback;
        private BufferedWaveProvider? _bufferProvider;
        private readonly object _sync = new();

        public event EventHandler<MeterReading>? InputMeterUpdated;
        public event EventHandler<MeterReading>? OutputMeterUpdated;
        public event EventHandler<float[]>? WaveformUpdated;
        public double PitchFactor { get; set; } = 1.0;
        public PresetMode Preset { get; private set; } = PresetMode.Clean;
        public bool IsRunning => _capture != null;
        public bool DirectMonitoringEnabled { get; set; }
        public double StereoWidth { get; set; } = 1.0;
        public int SampleRate { get; set; } = 48000;
        public int Channels { get; set; } = 1;
        public int BufferMilliseconds { get; set; } = 10;
        public int PlaybackLatencyMs { get; set; } = 80;

        public AudioEngine(EffectParameterSet parameters)
        {
            _parameters = parameters;
            _effectsProcessor = new EffectsProcessor(parameters);
        }

        public void SetPreset(PresetMode preset)
        {
            Preset = preset;
        }

        public void Start(int inputDevice, int outputDevice)
        {
            Stop();

            _capture = new WaveInEvent
            {
                DeviceNumber = inputDevice,
                WaveFormat = new WaveFormat(SampleRate, Channels),
                BufferMilliseconds = BufferMilliseconds,
                NumberOfBuffers = 6
            };

            _bufferProvider = new BufferedWaveProvider(_capture.WaveFormat)
            {
                DiscardOnBufferOverflow = true,
                BufferDuration = TimeSpan.FromSeconds(5)
            };

            _playback = new WaveOutEvent
            {
                DeviceNumber = outputDevice,
                DesiredLatency = PlaybackLatencyMs
            };

            _playback.Init(_bufferProvider);

            _capture.DataAvailable += OnDataAvailable;
            _capture.RecordingStopped += OnRecordingStopped;

            _playback.Play();
            _capture.StartRecording();
        }

        public void Stop()
        {
            lock (_sync)
            {
                if (_capture != null)
                {
                    _capture.DataAvailable -= OnDataAvailable;
                    _capture.RecordingStopped -= OnRecordingStopped;
                    _capture.StopRecording();
                    _capture.Dispose();
                    _capture = null;
                }

                if (_playback != null)
                {
                    _playback.Stop();
                    _playback.Dispose();
                    _playback = null;
                }

                _bufferProvider = null;
            }
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            Stop();
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (_bufferProvider == null || _capture == null)
            {
                return;
            }

            var channels = _capture.WaveFormat.Channels;
            var frames = e.BytesRecorded / _capture.WaveFormat.BlockAlign;
            EnsureBuffers(ref _inputBuffers, channels, frames);
            EnsureBuffers(ref _pitchBuffers, channels, frames);
            var waveBuffer = new WaveBuffer(e.Buffer);

            for (int frame = 0; frame < frames; frame++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    int idx = frame * channels + channel;
                    float sample = waveBuffer.ShortBuffer[idx] / 32768f;
                    _inputBuffers[channel][frame] = ApplyGain(sample, _parameters.InputGainDb);
                }
            }

            UpdateMeter(_inputBuffers, frames, InputMeterUpdated);

            for (int channel = 0; channel < channels; channel++)
            {
                ApplyPitchShift(_inputBuffers[channel], _pitchBuffers[channel], frames, PitchFactor);
                _effectsProcessor.Process(_pitchBuffers[channel], _capture.WaveFormat.SampleRate);
            }

            if (channels == 2)
            {
                ApplyStereoWidth(_pitchBuffers[0], _pitchBuffers[1], frames, StereoWidth);
            }

            var outputBuffer = new byte[frames * channels * 2];
            var outputWave = new WaveBuffer(outputBuffer);

            for (int frame = 0; frame < frames; frame++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    float processed = ApplyGain(_pitchBuffers[channel][frame], _parameters.OutputGainDb);
                    if (DirectMonitoringEnabled)
                    {
                        processed = (processed + _inputBuffers[channel][frame]) * 0.5f;
                    }

                    int idx = frame * channels + channel;
                    float clamped = Math.Clamp(processed, -1f, 1f);
                    outputWave.ShortBuffer[idx] = (short)(clamped * short.MaxValue);
                    _pitchBuffers[channel][frame] = clamped;
                }
            }

            _bufferProvider.AddSamples(outputBuffer, 0, outputBuffer.Length);
            UpdateMeter(_pitchBuffers, frames, OutputMeterUpdated);
            PublishWaveform(_pitchBuffers, frames);
        }

        private void ApplyPitchShift(float[] source, float[] destination, int length, double pitchFactor)
        {
            if (pitchFactor <= 0)
            {
                pitchFactor = 1.0;
            }

            for (int i = 0; i < length; i++)
            {
                double srcIndex = i / pitchFactor;
                int indexA = (int)Math.Floor(srcIndex);
                int indexB = Math.Min(indexA + 1, length - 1);
                double frac = srcIndex - indexA;

                float sampleA = source[Math.Clamp(indexA, 0, length - 1)];
                float sampleB = source[Math.Clamp(indexB, 0, length - 1)];
                destination[i] = (float)((1 - frac) * sampleA + frac * sampleB);
            }
        }

        private void PublishWaveform(float[][] buffers, int frames)
        {
            if (WaveformUpdated == null || buffers.Length == 0)
            {
                return;
            }

            int step = Math.Max(1, frames / 256);
            var points = new float[256];
            var source = buffers[0];
            for (int i = 0; i < points.Length; i++)
            {
                int idx = Math.Min(source.Length - 1, i * step);
                points[i] = source[idx];
            }

            WaveformUpdated.Invoke(this, points);
        }

        private void UpdateMeter(float[][] buffers, int frames, EventHandler<MeterReading>? handler)
        {
            if (handler == null || buffers.Length == 0)
            {
                return;
            }

            double sum = 0;
            float peak = 0;

            for (int frame = 0; frame < frames; frame++)
            {
                for (int channel = 0; channel < buffers.Length; channel++)
                {
                    float sample = buffers[channel][frame];
                    sum += sample * sample;
                    peak = Math.Max(peak, Math.Abs(sample));
                }
            }

            int totalSamples = frames * buffers.Length;
            float rms = (float)Math.Sqrt(sum / totalSamples);
            handler.Invoke(this, new MeterReading(rms, peak));
        }

        private static float ApplyGain(float sample, double gainDb)
        {
            double linear = Math.Pow(10, gainDb / 20.0);
            return (float)(sample * linear);
        }

        private static void EnsureBuffers(ref float[][] buffers, int channels, int size)
        {
            if (buffers.Length != channels)
            {
                buffers = new float[channels][];
            }

            for (int i = 0; i < channels; i++)
            {
                buffers[i] ??= Array.Empty<float>();
                if (buffers[i].Length < size)
                {
                    Array.Resize(ref buffers[i], size);
                }
            }
        }

        private static void ApplyStereoWidth(float[] left, float[] right, int frames, double width)
        {
            float widthFactor = (float)Math.Clamp(width, 0, 2);
            for (int i = 0; i < frames; i++)
            {
                float mid = (left[i] + right[i]) * 0.5f;
                float side = (left[i] - right[i]) * 0.5f * widthFactor;
                left[i] = mid + side;
                right[i] = mid - side;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public record struct MeterReading(float Rms, float Peak);
}
