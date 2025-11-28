using System;
using System.Linq;
using NAudio.Wave;

namespace VoiceQueen
{
    public class AudioEngine : IDisposable
    {
        private readonly EffectsProcessor _effectsProcessor = new();
        private WaveInEvent? _capture;
        private WaveOutEvent? _playback;
        private BufferedWaveProvider? _bufferProvider;
        private readonly object _sync = new();

        public event EventHandler<float>? LevelUpdated;
        public double PitchFactor { get; set; } = 1.0;
        public PresetMode Preset { get; private set; } = PresetMode.Clean;
        public bool IsRunning => _capture != null;

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
                BufferMilliseconds = 20,
                NumberOfBuffers = 4
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
            var inputFloats = new float[samples];
            var waveBuffer = new WaveBuffer(e.Buffer);
            for (int i = 0; i < samples; i++)
            {
                inputFloats[i] = waveBuffer.ShortBuffer[i] / 32768f;
            }

            var pitched = new float[samples];
            ApplyPitchShift(inputFloats, pitched, PitchFactor);
            _effectsProcessor.Process(pitched, Preset, _capture.WaveFormat.SampleRate);

            var outputBuffer = new byte[pitched.Length * 2];
            var outputWave = new WaveBuffer(outputBuffer);
            for (int i = 0; i < pitched.Length; i++)
            {
                float clamped = Math.Clamp(pitched[i], -1f, 1f);
                outputWave.ShortBuffer[i] = (short)(clamped * short.MaxValue);
            }

            _bufferProvider.AddSamples(outputBuffer, 0, outputBuffer.Length);
            UpdateLevel(pitched);
        }

        private void ApplyPitchShift(float[] source, float[] destination, double pitchFactor)
        {
            if (pitchFactor <= 0)
            {
                pitchFactor = 1.0;
            }

            for (int i = 0; i < destination.Length; i++)
            {
                double srcIndex = i / pitchFactor;
                int indexA = (int)Math.Floor(srcIndex);
                int indexB = Math.Min(indexA + 1, source.Length - 1);
                double frac = srcIndex - indexA;

                float sampleA = source[Math.Clamp(indexA, 0, source.Length - 1)];
                float sampleB = source[Math.Clamp(indexB, 0, source.Length - 1)];
                destination[i] = (float)((1 - frac) * sampleA + frac * sampleB);
            }
        }

        private void UpdateLevel(float[] buffer)
        {
            if (LevelUpdated == null || buffer.Length == 0)
            {
                return;
            }

            float rms = (float)Math.Sqrt(buffer.Select(x => x * x).Average());
            LevelUpdated.Invoke(this, rms);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
