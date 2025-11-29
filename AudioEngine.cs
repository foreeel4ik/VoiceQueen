using System;
using NAudio.Wave;

namespace VoiceQueen
{
    public class AudioEngine : IDisposable
    {
        private readonly EffectsProcessor _effectsProcessor;
        private readonly EffectParameterSet _parameters;
        private float[] _inputBuffer = Array.Empty<float>();
        private float[] _pitchBuffer = Array.Empty<float>();
        private WaveInEvent? _capture;
        private WaveOutEvent? _playback;
        private BufferedWaveProvider? _bufferProvider;
        private readonly object _sync = new();

        public event EventHandler<float>? LevelUpdated;
        public double PitchFactor { get; set; } = 1.0;
        public PresetMode Preset { get; private set; } = PresetMode.Clean;
        public bool IsRunning => _capture != null;

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
                WaveFormat = new WaveFormat(48000, 1),
                BufferMilliseconds = 10,
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
                DesiredLatency = 80
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

            var samples = e.BytesRecorded / 2;
            EnsureBuffer(ref _inputBuffer, samples);
            EnsureBuffer(ref _pitchBuffer, samples);
            var waveBuffer = new WaveBuffer(e.Buffer);
            for (int i = 0; i < samples; i++)
            {
                _inputBuffer[i] = waveBuffer.ShortBuffer[i] / 32768f;
            }

            ApplyPitchShift(_inputBuffer, _pitchBuffer, samples, PitchFactor);
            _effectsProcessor.Process(_pitchBuffer, _capture.WaveFormat.SampleRate);

            var outputBuffer = new byte[samples * 2];
            var outputWave = new WaveBuffer(outputBuffer);
            for (int i = 0; i < samples; i++)
            {
                float clamped = Math.Clamp(_pitchBuffer[i], -1f, 1f);
                outputWave.ShortBuffer[i] = (short)(clamped * short.MaxValue);
            }

            _bufferProvider.AddSamples(outputBuffer, 0, outputBuffer.Length);
            UpdateLevel(_pitchBuffer, samples);
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

        private void UpdateLevel(float[] buffer, int length)
        {
            if (LevelUpdated == null || buffer.Length == 0)
            {
                return;
            }

            float sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += buffer[i] * buffer[i];
            }

            float rms = (float)Math.Sqrt(sum / length);
            LevelUpdated.Invoke(this, rms);
        }

        private static void EnsureBuffer(ref float[] buffer, int size)
        {
            if (buffer.Length < size)
            {
                Array.Resize(ref buffer, size);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
