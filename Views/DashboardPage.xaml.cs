using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VoiceQueen;

namespace VoiceQueen.Views
{
    public partial class DashboardPage : Page
    {
        private readonly AudioDeviceManager _deviceManager = new();
        private readonly EffectParameterSet _parameters = new();
        private readonly AudioEngine _engine;
        private readonly AuthService _authService = new();

        public ObservableCollection<PresetViewModel> Presets { get; } = new();
        public EffectParameterSet Parameters => _parameters;

        public DashboardPage()
        {
            InitializeComponent();
            _engine = new AudioEngine(_parameters);
            DataContext = this;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            PitchSlider.ValueChanged += PitchSlider_ValueChanged;
            _engine.LevelUpdated += Engine_LevelUpdated;
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

        private void Engine_LevelUpdated(object? sender, float level)
        {
            Dispatcher.Invoke(() =>
            {
                LevelMeter.Value = level;
                LevelText.Text = $"Level: {(int)(level * 100)}%";
            });
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
    }
}
