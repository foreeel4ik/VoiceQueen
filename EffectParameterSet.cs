using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoiceQueen
{
    public enum DistortionMode
    {
        SoftClip,
        HardClip,
        Fuzz
    }

    public class EffectParameterSet : INotifyPropertyChanged
    {
        private bool _eqEnabled = true;
        private double _eqLowGain;
        private double _eqMidGain;
        private double _eqHighGain;

        private double _inputGainDb;
        private double _outputGainDb;

        private bool _reverbEnabled = true;
        private double _reverbMix = 0.12;
        private double _reverbDecay = 0.45;

        private bool _delayEnabled;
        private double _delayTimeMs = 180;
        private double _delayFeedback = 0.25;
        private double _delayMix = 0.18;

        private bool _chorusEnabled;
        private double _chorusDepthMs = 8;
        private double _chorusRate = 0.9;

        private bool _formantEnabled;
        private double _formantShift;

        private bool _noiseGateEnabled = true;
        private double _noiseGateThreshold = 0.02;
        private double _noiseGateAttackMs = 5;
        private double _noiseGateReleaseMs = 80;

        private bool _compressorEnabled = true;
        private double _compressorThreshold = -12;
        private double _compressorRatio = 3.5;
        private double _compressorAttackMs = 10;
        private double _compressorReleaseMs = 60;
        private double _compressorKneeDb = 6;
        private double _compressorMakeupDb = 2;

        private double _stereoWidth = 1.0;

        private bool _distortionEnabled;
        private double _distortionDrive = 1.2;
        private double _distortionMix = 0.35;
        private DistortionMode _distortionType = DistortionMode.SoftClip;

        public bool EqEnabled
        {
            get => _eqEnabled;
            set => SetField(ref _eqEnabled, value);
        }

        public double EqLowGain
        {
            get => _eqLowGain;
            set => SetField(ref _eqLowGain, value);
        }

        public double EqMidGain
        {
            get => _eqMidGain;
            set => SetField(ref _eqMidGain, value);
        }

        public double EqHighGain
        {
            get => _eqHighGain;
            set => SetField(ref _eqHighGain, value);
        }

        public double InputGainDb
        {
            get => _inputGainDb;
            set => SetField(ref _inputGainDb, value);
        }

        public double OutputGainDb
        {
            get => _outputGainDb;
            set => SetField(ref _outputGainDb, value);
        }

        public bool ReverbEnabled
        {
            get => _reverbEnabled;
            set => SetField(ref _reverbEnabled, value);
        }

        public double ReverbMix
        {
            get => _reverbMix;
            set => SetField(ref _reverbMix, value);
        }

        public double ReverbDecay
        {
            get => _reverbDecay;
            set => SetField(ref _reverbDecay, value);
        }

        public bool DelayEnabled
        {
            get => _delayEnabled;
            set => SetField(ref _delayEnabled, value);
        }

        public double DelayTimeMs
        {
            get => _delayTimeMs;
            set => SetField(ref _delayTimeMs, value);
        }

        public double DelayFeedback
        {
            get => _delayFeedback;
            set => SetField(ref _delayFeedback, value);
        }

        public double DelayMix
        {
            get => _delayMix;
            set => SetField(ref _delayMix, value);
        }

        public bool ChorusEnabled
        {
            get => _chorusEnabled;
            set => SetField(ref _chorusEnabled, value);
        }

        public double ChorusDepthMs
        {
            get => _chorusDepthMs;
            set => SetField(ref _chorusDepthMs, value);
        }

        public double ChorusRate
        {
            get => _chorusRate;
            set => SetField(ref _chorusRate, value);
        }

        public bool FormantEnabled
        {
            get => _formantEnabled;
            set => SetField(ref _formantEnabled, value);
        }

        public double FormantShift
        {
            get => _formantShift;
            set => SetField(ref _formantShift, value);
        }

        public bool NoiseGateEnabled
        {
            get => _noiseGateEnabled;
            set => SetField(ref _noiseGateEnabled, value);
        }

        public double NoiseGateThreshold
        {
            get => _noiseGateThreshold;
            set => SetField(ref _noiseGateThreshold, value);
        }

        public double NoiseGateAttackMs
        {
            get => _noiseGateAttackMs;
            set => SetField(ref _noiseGateAttackMs, value);
        }

        public double NoiseGateReleaseMs
        {
            get => _noiseGateReleaseMs;
            set => SetField(ref _noiseGateReleaseMs, value);
        }

        public bool CompressorEnabled
        {
            get => _compressorEnabled;
            set => SetField(ref _compressorEnabled, value);
        }

        public double CompressorThreshold
        {
            get => _compressorThreshold;
            set => SetField(ref _compressorThreshold, value);
        }

        public double CompressorRatio
        {
            get => _compressorRatio;
            set => SetField(ref _compressorRatio, value);
        }

        public double CompressorAttackMs
        {
            get => _compressorAttackMs;
            set => SetField(ref _compressorAttackMs, value);
        }

        public double CompressorReleaseMs
        {
            get => _compressorReleaseMs;
            set => SetField(ref _compressorReleaseMs, value);
        }

        public double CompressorKneeDb
        {
            get => _compressorKneeDb;
            set => SetField(ref _compressorKneeDb, value);
        }

        public double CompressorMakeupDb
        {
            get => _compressorMakeupDb;
            set => SetField(ref _compressorMakeupDb, value);
        }

        public double StereoWidth
        {
            get => _stereoWidth;
            set => SetField(ref _stereoWidth, value);
        }

        public bool DistortionEnabled
        {
            get => _distortionEnabled;
            set => SetField(ref _distortionEnabled, value);
        }

        public double DistortionDrive
        {
            get => _distortionDrive;
            set => SetField(ref _distortionDrive, value);
        }

        public double DistortionMix
        {
            get => _distortionMix;
            set => SetField(ref _distortionMix, value);
        }

        public DistortionMode DistortionType
        {
            get => _distortionType;
            set => SetField(ref _distortionType, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
