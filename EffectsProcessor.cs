using System;

namespace VoiceQueen
{
    public class EffectsProcessor
    {
        private double _ringPhase;
        private float _radioLowState;
        private float _radioHighState;
        private readonly Random _noiseRandom = new();

        public void Process(float[] buffer, PresetMode preset, int sampleRate)
        {
            switch (preset)
            {
                case PresetMode.Clean:
                    break;
                case PresetMode.Demon:
                    ApplyDemon(buffer);
                    break;
                case PresetMode.Robot:
                    ApplyRobot(buffer, sampleRate);
                    break;
                case PresetMode.Radio:
                    ApplyRadio(buffer, sampleRate);
                    break;
            }
        }

        private void ApplyDemon(float[] buffer)
        {
            const float drive = 1.8f;
            const float saturation = 0.7f;
            for (int i = 0; i < buffer.Length; i++)
            {
                var sample = buffer[i] * drive;
                sample = (float)Math.Tanh(sample * saturation);
                buffer[i] = sample * 0.9f;
            }
        }

        private void ApplyRobot(float[] buffer, int sampleRate)
        {
            const float modulationFreq = 30f;
            for (int i = 0; i < buffer.Length; i++)
            {
                var modulator = (float)Math.Sin(_ringPhase);
                buffer[i] *= modulator;
                _ringPhase += 2 * Math.PI * modulationFreq / sampleRate;
                if (_ringPhase > Math.PI * 2)
                {
                    _ringPhase -= (float)(Math.PI * 2);
                }
            }
        }

        private void ApplyRadio(float[] buffer, int sampleRate)
        {
            float lowCut = 0.1f;
            float highCut = 0.07f;
            float noiseLevel = 0.01f;

            for (int i = 0; i < buffer.Length; i++)
            {
                _radioLowState += lowCut * (buffer[i] - _radioLowState);
                float highPassed = buffer[i] - _radioLowState;

                _radioHighState += highCut * (highPassed - _radioHighState);
                float bandPassed = _radioHighState;

                float noise = (float)(_noiseRandom.NextDouble() * 2 - 1) * noiseLevel;
                buffer[i] = (bandPassed * 0.8f) + noise;
            }
        }
    }
}
