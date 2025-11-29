using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceQueen;

namespace VoiceQueen.Views
{
    public partial class DashboardPage : Page, System.ComponentModel.INotifyPropertyChanged
    {
        private readonly AudioDeviceManager _deviceManager = new();
        private readonly EffectParameterSet _parameters = new();
        private readonly AudioEngine _engine;
        private readonly AuthService _authService = new();

        public ObservableCollection<PresetViewModel> Presets { get; } = new();
        public EffectParameterSet Parameters => _parameters;
        public ObservableCollection<int> SampleRates { get; } = new(new[] { 44100, 48000, 96000 });
        public ObservableCollection<int> ChannelOptions { get; } = new(new[] { 1, 2 });
        public ObservableCollection<int> BufferOptions { get; } = new(new[] { 5, 10, 20, 40 });
        public ObservableCollection<int> LatencyOptions { get; } = new(new[] { 40, 60, 80, 120 });

        private int _selectedSampleRate = 48000;
        private int _selectedChannels = 1;
        private int _selectedBufferMs = 10;
        private int _selectedLatencyMs = 80;
        private bool _directMonitoringEnabled;
        private double _inputMeterRms;
        private double _inputMeterPeak;
        private double _outputMeterRms;
        private double _outputMeterPeak;
        private PointCollection _waveformPoints = new();

        public DashboardPage()
        {
            InitializeComponent();
            _engine = new AudioEngine(_parameters);
            DataContext = this;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            PitchSlider.ValueChanged += PitchSlider_ValueChanged;
            _engine.InputMeterUpdated += Engine_InputMeterUpdated;
            _engine.OutputMeterUpdated += Engine_OutputMeterUpdated;
            _engine.WaveformUpdated += Engine_WaveformUpdated;
            _parameters.PropertyChanged += Parameters_PropertyChanged;
            InitializePresets();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InputDevices.ItemsSource = _deviceManager.GetInputDevices();
            InputDevices.DisplayMemberPath = nameof(AudioDeviceInfo.Name);
            OutputDevices.ItemsSource = _deviceManager.GetOutputDevices();
            OutputDevices.DisplayMemberPath = nameof(AudioDeviceInfo.Name);

            if (InputDevices.Items.Count > 0)
            {
                InputDevices.SelectedIndex = 0;
            }

            if (OutputDevices.Items.Count > 0)
            {
                OutputDevices.SelectedIndex = 0;
            }

            var initialPreset = Presets.FirstOrDefault() ?? Presets.FirstOrDefault(p => p.Mode == PresetMode.Clean);
            if (initialPreset != null)
            {
                SetPreset(initialPreset);
            }
            UpdatePitch(PitchSlider.Value);
            ApplyEngineSettings();
        }

        private void PitchSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePitch(e.NewValue);
        }

        private void UpdatePitch(double value)
        {
            _engine.PitchFactor = value;
        }

        private void PresetCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: PresetViewModel preset })
            {
                SetPreset(preset);
            }
        }

        private void InitializePresets()
        {
            Presets.Add(new PresetViewModel
            {
                Title = "Natural Voice",
                Subtitle = "Uncolored and transparent",
                Category = "Neutral",
                Mode = PresetMode.Clean
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Demon Lord",
                Subtitle = "Grit and growl",
                Category = "Demon",
                Mode = PresetMode.Demon
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Alien Overmind",
                Subtitle = "Otherworldly resonance",
                Category = "Sci-Fi",
                Mode = PresetMode.Alien
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Vintage Radio",
                Subtitle = "Warm broadcast",
                Category = "Radio",
                Mode = PresetMode.Radio
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Star Wanderer",
                Subtitle = "Ethereal echoes",
                Category = "Galaxy",
                Mode = PresetMode.Storyteller
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Whisper AI",
                Subtitle = "Soft storyteller",
                Category = "Whisper",
                Mode = PresetMode.Whisper
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Storyteller",
                Subtitle = "Cinematic depth",
                Category = "Narrator",
                Mode = PresetMode.Narrator
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Studio Vocal",
                Subtitle = "Polished presence",
                Category = "Studio",
                Mode = PresetMode.Studio
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Chrome Automaton",
                Subtitle = "Synthetic cadence",
                Category = "Robot",
                Mode = PresetMode.Robot
            });

            Presets.Add(new PresetViewModel
            {
                Title = "Child Explorer",
                Subtitle = "Bright and playful",
                Category = "Child",
                Mode = PresetMode.Child
            });
        }

        private void SetPreset(PresetViewModel preset)
        {
            _engine.SetPreset(preset.Mode);
            PresetConfigurations.Apply(preset.Mode, _parameters);
            var config = PresetConfigurations.Get(preset.Mode);
            PitchSlider.Value = config.Pitch;

            foreach (var card in Presets)
            {
                card.IsSelected = ReferenceEquals(card, preset);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AppState.Instance.IsAuthenticated)
            {
                MessageBox.Show("Please login before starting audio.", "VoiceQueen", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (InputDevices.SelectedItem is not AudioDeviceInfo input || OutputDevices.SelectedItem is not AudioDeviceInfo output)
            {
                MessageBox.Show("Select input and output devices first.", "VoiceQueen", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ApplyEngineSettings();
                _engine.Start(input.Id, output.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start audio: {ex.Message}", "VoiceQueen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _engine.Stop();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _engine.Dispose();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            _engine.Stop();
            NavigationService?.Navigate(new LoginPage());
        }

        private void ApplyEngineSettings()
        {
            _engine.SampleRate = SelectedSampleRate;
            _engine.Channels = SelectedChannels;
            _engine.BufferMilliseconds = SelectedBufferMs;
            _engine.PlaybackLatencyMs = SelectedLatencyMs;
            _engine.DirectMonitoringEnabled = DirectMonitoringEnabled;
            _engine.StereoWidth = _parameters.StereoWidth;
        }

        private void Parameters_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EffectParameterSet.StereoWidth))
            {
                _engine.StereoWidth = _parameters.StereoWidth;
            }
        }

        private void RestartIfRunning()
        {
            if (!_engine.IsRunning)
            {
                return;
            }

            if (InputDevices.SelectedItem is not AudioDeviceInfo input || OutputDevices.SelectedItem is not AudioDeviceInfo output)
            {
                return;
            }

            _engine.Stop();
            ApplyEngineSettings();
            _engine.Start(input.Id, output.Id);
        }

        private void Engine_InputMeterUpdated(object? sender, MeterReading e)
        {
            Dispatcher.Invoke(() =>
            {
                InputMeterRms = e.Rms;
                InputMeterPeak = e.Peak;
            });
        }

        private void Engine_OutputMeterUpdated(object? sender, MeterReading e)
        {
            Dispatcher.Invoke(() =>
            {
                OutputMeterRms = e.Rms;
                OutputMeterPeak = e.Peak;
            });
        }

        private void Engine_WaveformUpdated(object? sender, float[] samples)
        {
            Dispatcher.Invoke(() =>
            {
                var points = new PointCollection();
                for (int i = 0; i < samples.Length; i++)
                {
                    points.Add(new Point(i, (1 - samples[i]) * 40));
                }

                WaveformPoints = points;
            });
        }

        public int SelectedSampleRate
        {
            get => _selectedSampleRate;
            set
            {
                if (_selectedSampleRate != value)
                {
                    _selectedSampleRate = value;
                    OnPropertyChanged(nameof(SelectedSampleRate));
                    ApplyEngineSettings();
                    RestartIfRunning();
                }
            }
        }

        public int SelectedChannels
        {
            get => _selectedChannels;
            set
            {
                if (_selectedChannels != value)
                {
                    _selectedChannels = value;
                    OnPropertyChanged(nameof(SelectedChannels));
                    ApplyEngineSettings();
                    RestartIfRunning();
                }
            }
        }

        public int SelectedBufferMs
        {
            get => _selectedBufferMs;
            set
            {
                if (_selectedBufferMs != value)
                {
                    _selectedBufferMs = value;
                    OnPropertyChanged(nameof(SelectedBufferMs));
                    ApplyEngineSettings();
                    RestartIfRunning();
                }
            }
        }

        public int SelectedLatencyMs
        {
            get => _selectedLatencyMs;
            set
            {
                if (_selectedLatencyMs != value)
                {
                    _selectedLatencyMs = value;
                    OnPropertyChanged(nameof(SelectedLatencyMs));
                    ApplyEngineSettings();
                    RestartIfRunning();
                }
            }
        }

        public bool DirectMonitoringEnabled
        {
            get => _directMonitoringEnabled;
            set
            {
                if (_directMonitoringEnabled != value)
                {
                    _directMonitoringEnabled = value;
                    OnPropertyChanged(nameof(DirectMonitoringEnabled));
                    _engine.DirectMonitoringEnabled = value;
                }
            }
        }

        public double InputMeterRms
        {
            get => _inputMeterRms;
            private set
            {
                _inputMeterRms = value;
                OnPropertyChanged(nameof(InputMeterRms));
            }
        }

        public double InputMeterPeak
        {
            get => _inputMeterPeak;
            private set
            {
                _inputMeterPeak = value;
                OnPropertyChanged(nameof(InputMeterPeak));
            }
        }

        public double OutputMeterRms
        {
            get => _outputMeterRms;
            private set
            {
                _outputMeterRms = value;
                OnPropertyChanged(nameof(OutputMeterRms));
            }
        }

        public double OutputMeterPeak
        {
            get => _outputMeterPeak;
            private set
            {
                _outputMeterPeak = value;
                OnPropertyChanged(nameof(OutputMeterPeak));
            }
        }

        public PointCollection WaveformPoints
        {
            get => _waveformPoints;
            private set
            {
                _waveformPoints = value;
                OnPropertyChanged(nameof(WaveformPoints));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
