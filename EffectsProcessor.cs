using System;

namespace VoiceQueen
{
    public class EffectsProcessor
    {
        private readonly EffectChain _chain;

        public EffectsProcessor(EffectParameterSet parameters)
        {
            _chain = new EffectChain(
                new NoiseGateEffect(parameters),
                new CompressorEffect(parameters),
                new EqualizerEffect(parameters),
                new FormantShiftEffect(parameters),
                new ChorusEffect(parameters),
                new DelayEffect(parameters),
                new ReverbEffect(parameters),
                new DistortionEffect(parameters)
            );
        }

        public void Process(float[] buffer, int sampleRate)
        {
            _chain.Process(buffer, sampleRate);
        }
    }
}
