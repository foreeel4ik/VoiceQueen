using System;

namespace VoiceQueen
{
    public interface IAudioEffect
    {
        void Process(float[] buffer, int sampleRate);
    }

    public class EffectChain
    {
        private readonly IAudioEffect[] _effects;

        public EffectChain(params IAudioEffect[] effects)
        {
            _effects = effects;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            foreach (var effect in _effects)
            {
                effect.Process(buffer, sampleRate);
            }
        }
    }

    public class EqualizerEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private double _lowState;
        private double _highState;

        public EqualizerEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.EqEnabled)
            {
                return;
            }

            double lowGain = DbToLinear(_parameters.EqLowGain);
            double midGain = DbToLinear(_parameters.EqMidGain);
            double highGain = DbToLinear(_parameters.EqHighGain);

            double lowCoeff = 2 * Math.PI * 120 / sampleRate;
            double highCoeff = 2 * Math.PI * 4000 / sampleRate;

            for (int i = 0; i < buffer.Length; i++)
            {
                _lowState += lowCoeff * (buffer[i] - _lowState);
                double highPassed = buffer[i] - _lowState;

                _highState += highCoeff * (highPassed - _highState);
                double bandPassed = highPassed - _highState;
                double highShelf = _highState;

                double eqSample = (_lowState * lowGain) + (bandPassed * midGain) + (highShelf * highGain);
                buffer[i] = (float)eqSample;
            }
        }

        private static double DbToLinear(double db)
        {
            return Math.Pow(10, db / 20.0);
        }
    }

    public class ReverbEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private readonly float[][] _delayLines;
        private readonly int[] _indices;

        public ReverbEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
            _delayLines = new[]
            {
                new float[2048],
                new float[2753],
                new float[3173]
            };
            _indices = new int[_delayLines.Length];
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.ReverbEnabled)
            {
                return;
            }

            float mix = (float)_parameters.ReverbMix;
            float decay = (float)_parameters.ReverbDecay;

            for (int i = 0; i < buffer.Length; i++)
            {
                float input = buffer[i];
                float accum = 0;

                for (int line = 0; line < _delayLines.Length; line++)
                {
                    var delay = _delayLines[line];
                    int idx = _indices[line];
                    float delayed = delay[idx];

                    delay[idx] = input + delayed * decay;
                    _indices[line] = (idx + 1) % delay.Length;
                    accum += delayed;
                }

                float wet = accum / _delayLines.Length;
                buffer[i] = (input * (1 - mix)) + (wet * mix);
            }
        }
    }

    public class DelayEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private float[] _buffer = new float[1];
        private int _writeIndex;

        public DelayEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.DelayEnabled)
            {
                return;
            }

            double delayTimeMs = Math.Clamp(_parameters.DelayTimeMs, 10, 1500);
            int delaySamples = (int)(sampleRate * delayTimeMs / 1000.0);
            EnsureBuffer(delaySamples + buffer.Length);

            float feedback = (float)Math.Clamp(_parameters.DelayFeedback, 0, 0.95);
            float mix = (float)Math.Clamp(_parameters.DelayMix, 0, 1);

            for (int i = 0; i < buffer.Length; i++)
            {
                int readIndex = (_writeIndex - delaySamples + _buffer.Length) % _buffer.Length;
                float delayed = _buffer[readIndex];

                _buffer[_writeIndex] = buffer[i] + delayed * feedback;
                _writeIndex = (_writeIndex + 1) % _buffer.Length;

                buffer[i] = (buffer[i] * (1 - mix)) + (delayed * mix);
            }
        }

        private void EnsureBuffer(int size)
        {
            if (_buffer.Length >= size)
            {
                return;
            }

            Array.Resize(ref _buffer, size);
        }
    }

    public class ChorusEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private float[] _delayBuffer = new float[1];
        private int _writeIndex;
        private double _phase;

        public ChorusEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.ChorusEnabled)
            {
                return;
            }

            float depthMs = (float)Math.Clamp(_parameters.ChorusDepthMs, 1, 20);
            float rateHz = (float)Math.Clamp(_parameters.ChorusRate, 0.1, 5);
            int maxDelaySamples = (int)(sampleRate * (depthMs / 1000.0) * 2);
            EnsureBuffer(maxDelaySamples + buffer.Length + 1);

            for (int i = 0; i < buffer.Length; i++)
            {
                double lfo = Math.Sin(_phase) * 0.5 + 0.5;
                double modDelay = (depthMs / 1000.0) * (0.5 + lfo);
                int delaySamples = Math.Max(1, (int)(sampleRate * modDelay));

                int readIndex = (_writeIndex - delaySamples + _delayBuffer.Length) % _delayBuffer.Length;
                float delayed = _delayBuffer[readIndex];

                _delayBuffer[_writeIndex] = buffer[i];
                _writeIndex = (_writeIndex + 1) % _delayBuffer.Length;

                buffer[i] = (buffer[i] + delayed) * 0.5f;
                _phase += 2 * Math.PI * rateHz / sampleRate;
                if (_phase > Math.PI * 2)
                {
                    _phase -= Math.PI * 2;
                }
            }
        }

        private void EnsureBuffer(int size)
        {
            if (_delayBuffer.Length >= size)
            {
                return;
            }

            Array.Resize(ref _delayBuffer, size);
        }
    }

    public class FormantShiftEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private double _tiltState;

        public FormantShiftEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.FormantEnabled || Math.Abs(_parameters.FormantShift) < 0.001)
            {
                return;
            }

            double shift = Math.Clamp(_parameters.FormantShift, -12, 12);
            double tilt = shift / 24.0;
            double coeff = 2 * Math.PI * 900 / sampleRate;

            for (int i = 0; i < buffer.Length; i++)
            {
                _tiltState += coeff * (buffer[i] - _tiltState);
                double highPassed = buffer[i] - _tiltState;
                buffer[i] = (float)((buffer[i] * (1 - tilt)) + (highPassed * tilt));
            }
        }
    }

    public class NoiseGateEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private float _envelope;
        private float _gateGain;

        public NoiseGateEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
            _gateGain = 1f;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.NoiseGateEnabled)
            {
                return;
            }

            float threshold = (float)Math.Clamp(_parameters.NoiseGateThreshold, 0.001, 0.2);
            float attackCoeff = (float)Math.Exp(-1.0 / (sampleRate * Math.Max(0.001, _parameters.NoiseGateAttackMs / 1000.0)));
            float releaseCoeff = (float)Math.Exp(-1.0 / (sampleRate * Math.Max(0.001, _parameters.NoiseGateReleaseMs / 1000.0)));

            for (int i = 0; i < buffer.Length; i++)
            {
                float level = Math.Abs(buffer[i]);
                _envelope = Math.Max(level, _envelope * releaseCoeff);

                float target = _envelope < threshold ? 0f : 1f;
                float coeff = target > _gateGain ? attackCoeff : releaseCoeff;
                _gateGain = target + (_gateGain - target) * coeff;
                buffer[i] *= _gateGain;
            }
        }
    }

    public class CompressorEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;
        private float _gain;

        public CompressorEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
            _gain = 1f;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.CompressorEnabled)
            {
                return;
            }

            float ratio = (float)Math.Max(1.0, _parameters.CompressorRatio);
            float attackCoeff = (float)Math.Exp(-1.0 / (sampleRate * (_parameters.CompressorAttackMs / 1000.0)));
            float releaseCoeff = (float)Math.Exp(-1.0 / (sampleRate * (_parameters.CompressorReleaseMs / 1000.0)));
            float kneeDb = (float)Math.Max(0, _parameters.CompressorKneeDb);
            float halfKnee = kneeDb / 2f;
            float makeup = (float)Math.Pow(10, _parameters.CompressorMakeupDb / 20.0);

            for (int i = 0; i < buffer.Length; i++)
            {
                float level = Math.Abs(buffer[i]);
                float inputDb = 20f * (float)Math.Log10(Math.Max(level, 1e-6));
                float thresholdDb = _parameters.CompressorThreshold;

                float gainDb;
                if (kneeDb > 0 && inputDb > thresholdDb - halfKnee && inputDb < thresholdDb + halfKnee)
                {
                    float delta = inputDb - (thresholdDb - halfKnee);
                    float proportion = delta / kneeDb;
                    float compressedDb = inputDb + (1 / ratio - 1) * proportion * proportion * delta;
                    gainDb = compressedDb - inputDb;
                }
                else if (inputDb > thresholdDb)
                {
                    gainDb = thresholdDb + (inputDb - thresholdDb) / ratio - inputDb;
                }
                else
                {
                    gainDb = 0;
                }

                float targetGain = (float)Math.Pow(10, gainDb / 20.0) * makeup;
                float coeff = targetGain < _gain ? attackCoeff : releaseCoeff;
                _gain = targetGain + (_gain - targetGain) * coeff;
                buffer[i] *= _gain;
            }
        }
    }

    public class DistortionEffect : IAudioEffect
    {
        private readonly EffectParameterSet _parameters;

        public DistortionEffect(EffectParameterSet parameters)
        {
            _parameters = parameters;
        }

        public void Process(float[] buffer, int sampleRate)
        {
            if (!_parameters.DistortionEnabled)
            {
                return;
            }

            float drive = (float)Math.Max(1.0, _parameters.DistortionDrive);
            float mix = (float)Math.Clamp(_parameters.DistortionMix, 0, 1);

            for (int i = 0; i < buffer.Length; i++)
            {
                float driven = buffer[i] * drive;
                float shaped = _parameters.DistortionType switch
                {
                    DistortionMode.Fuzz => (float)Math.Tanh(driven * 1.8f),
                    DistortionMode.HardClip => Math.Clamp(driven, -0.6f, 0.6f),
                    _ => (float)(driven / (1 + Math.Abs(driven)))
                };

                buffer[i] = (buffer[i] * (1 - mix)) + (shaped * mix);
            }
        }
    }
}
