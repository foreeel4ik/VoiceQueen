using System;
using System.Collections.Generic;

namespace VoiceQueen
{
    public class PresetConfiguration
    {
        public double Pitch { get; init; } = 1.0;
        public Action<EffectParameterSet> Configure { get; init; } = _ => { };
    }

    public static class PresetConfigurations
    {
        private static readonly Dictionary<PresetMode, PresetConfiguration> PresetMap = new()
        {
            [PresetMode.Clean] = new PresetConfiguration
            {
                Pitch = 1.0,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.NoiseGateEnabled = true;
                    parameters.CompressorEnabled = true;
                    parameters.EqEnabled = true;
                    parameters.DelayEnabled = false;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.1;
                    parameters.ReverbDecay = 0.35;
                    parameters.ChorusEnabled = false;
                    parameters.FormantEnabled = false;
                    parameters.DistortionEnabled = false;
                }
            },
            [PresetMode.Demon] = new PresetConfiguration
            {
                Pitch = 0.8,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.FormantEnabled = true;
                    parameters.FormantShift = -6;
                    parameters.EqEnabled = true;
                    parameters.EqLowGain = 4;
                    parameters.EqHighGain = -4;
                    parameters.DelayEnabled = true;
                    parameters.DelayTimeMs = 320;
                    parameters.DelayFeedback = 0.3;
                    parameters.DelayMix = 0.22;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.18;
                    parameters.ReverbDecay = 0.5;
                    parameters.DistortionEnabled = true;
                    parameters.DistortionDrive = 1.8;
                    parameters.DistortionMix = 0.55;
                    parameters.DistortionType = DistortionMode.Fuzz;
                }
            },
            [PresetMode.Robot] = new PresetConfiguration
            {
                Pitch = 1.0,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.FormantEnabled = true;
                    parameters.FormantShift = 3;
                    parameters.ChorusEnabled = true;
                    parameters.ChorusDepthMs = 6;
                    parameters.ChorusRate = 2.2;
                    parameters.DelayEnabled = false;
                    parameters.ReverbEnabled = false;
                    parameters.DistortionEnabled = true;
                    parameters.DistortionDrive = 1.3;
                    parameters.DistortionMix = 0.3;
                    parameters.DistortionType = DistortionMode.HardClip;
                }
            },
            [PresetMode.Radio] = new PresetConfiguration
            {
                Pitch = 1.0,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.EqEnabled = true;
                    parameters.EqLowGain = -6;
                    parameters.EqMidGain = -2;
                    parameters.EqHighGain = 3;
                    parameters.DelayEnabled = false;
                    parameters.ReverbEnabled = false;
                    parameters.NoiseGateEnabled = true;
                    parameters.NoiseGateThreshold = 0.04;
                    parameters.CompressorEnabled = true;
                }
            },
            [PresetMode.Child] = new PresetConfiguration
            {
                Pitch = 1.25,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.FormantEnabled = true;
                    parameters.FormantShift = 5;
                    parameters.ChorusEnabled = true;
                    parameters.ChorusDepthMs = 10;
                    parameters.ChorusRate = 1.6;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.16;
                    parameters.ReverbDecay = 0.4;
                    parameters.DelayEnabled = false;
                    parameters.DistortionEnabled = false;
                    parameters.EqEnabled = true;
                    parameters.EqHighGain = 3;
                }
            },
            [PresetMode.Whisper] = new PresetConfiguration
            {
                Pitch = 1.0,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.NoiseGateEnabled = true;
                    parameters.NoiseGateThreshold = 0.06;
                    parameters.CompressorEnabled = false;
                    parameters.EqEnabled = true;
                    parameters.EqLowGain = -8;
                    parameters.EqHighGain = 5;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.28;
                    parameters.ReverbDecay = 0.6;
                    parameters.DelayEnabled = false;
                    parameters.ChorusEnabled = false;
                    parameters.FormantEnabled = false;
                    parameters.DistortionEnabled = false;
                }
            },
            [PresetMode.Alien] = new PresetConfiguration
            {
                Pitch = 0.9,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.FormantEnabled = true;
                    parameters.FormantShift = -9;
                    parameters.ChorusEnabled = true;
                    parameters.ChorusDepthMs = 12;
                    parameters.ChorusRate = 0.8;
                    parameters.DelayEnabled = true;
                    parameters.DelayTimeMs = 420;
                    parameters.DelayFeedback = 0.35;
                    parameters.DelayMix = 0.28;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.22;
                    parameters.ReverbDecay = 0.7;
                    parameters.DistortionEnabled = true;
                    parameters.DistortionDrive = 1.4;
                    parameters.DistortionMix = 0.25;
                    parameters.DistortionType = DistortionMode.SoftClip;
                }
            },
            [PresetMode.Studio] = new PresetConfiguration
            {
                Pitch = 1.0,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.NoiseGateEnabled = true;
                    parameters.NoiseGateThreshold = 0.018;
                    parameters.CompressorEnabled = true;
                    parameters.CompressorThreshold = -9;
                    parameters.CompressorRatio = 2.8;
                    parameters.EqEnabled = true;
                    parameters.EqLowGain = 2;
                    parameters.EqMidGain = -1;
                    parameters.EqHighGain = 2;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.08;
                    parameters.ReverbDecay = 0.3;
                    parameters.DelayEnabled = false;
                    parameters.ChorusEnabled = false;
                    parameters.FormantEnabled = false;
                    parameters.DistortionEnabled = false;
                }
            },
            [PresetMode.Narrator] = new PresetConfiguration
            {
                Pitch = 0.95,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.NoiseGateEnabled = true;
                    parameters.NoiseGateThreshold = 0.025;
                    parameters.CompressorEnabled = true;
                    parameters.CompressorThreshold = -10;
                    parameters.CompressorRatio = 2.4;
                    parameters.EqEnabled = true;
                    parameters.EqLowGain = 3;
                    parameters.EqMidGain = 1;
                    parameters.EqHighGain = -1;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.12;
                    parameters.ReverbDecay = 0.45;
                    parameters.DelayEnabled = false;
                    parameters.ChorusEnabled = false;
                    parameters.FormantEnabled = false;
                    parameters.DistortionEnabled = false;
                }
            },
            [PresetMode.Storyteller] = new PresetConfiguration
            {
                Pitch = 1.05,
                Configure = parameters =>
                {
                    Reset(parameters);
                    parameters.NoiseGateEnabled = true;
                    parameters.NoiseGateThreshold = 0.022;
                    parameters.CompressorEnabled = true;
                    parameters.CompressorThreshold = -8;
                    parameters.CompressorRatio = 2.2;
                    parameters.EqEnabled = true;
                    parameters.EqLowGain = 1;
                    parameters.EqMidGain = 2;
                    parameters.EqHighGain = 2;
                    parameters.ReverbEnabled = true;
                    parameters.ReverbMix = 0.2;
                    parameters.ReverbDecay = 0.55;
                    parameters.DelayEnabled = true;
                    parameters.DelayTimeMs = 260;
                    parameters.DelayFeedback = 0.22;
                    parameters.DelayMix = 0.18;
                    parameters.ChorusEnabled = true;
                    parameters.ChorusDepthMs = 7;
                    parameters.ChorusRate = 1.1;
                    parameters.FormantEnabled = false;
                    parameters.DistortionEnabled = false;
                }
            }
        };

        public static PresetConfiguration Get(PresetMode mode)
        {
            return PresetMap.TryGetValue(mode, out var config) ? config : PresetMap[PresetMode.Clean];
        }

        public static void Apply(PresetMode mode, EffectParameterSet parameters)
        {
            Get(mode).Configure(parameters);
        }

        private static void Reset(EffectParameterSet parameters)
        {
            parameters.NoiseGateEnabled = true;
            parameters.NoiseGateThreshold = 0.02;
            parameters.CompressorEnabled = true;
            parameters.CompressorThreshold = -12;
            parameters.CompressorRatio = 3.5;
            parameters.CompressorAttackMs = 10;
            parameters.CompressorReleaseMs = 60;
            parameters.EqEnabled = true;
            parameters.EqLowGain = 0;
            parameters.EqMidGain = 0;
            parameters.EqHighGain = 0;
            parameters.ReverbEnabled = true;
            parameters.ReverbMix = 0.12;
            parameters.ReverbDecay = 0.45;
            parameters.DelayEnabled = false;
            parameters.DelayTimeMs = 180;
            parameters.DelayFeedback = 0.25;
            parameters.DelayMix = 0.18;
            parameters.ChorusEnabled = false;
            parameters.ChorusDepthMs = 8;
            parameters.ChorusRate = 0.9;
            parameters.FormantEnabled = false;
            parameters.FormantShift = 0;
            parameters.DistortionEnabled = false;
            parameters.DistortionDrive = 1.2;
            parameters.DistortionMix = 0.35;
            parameters.DistortionType = DistortionMode.SoftClip;
        }
    }
}
