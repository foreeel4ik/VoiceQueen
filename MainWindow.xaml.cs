using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VoiceQueen
{
    public partial class MainWindow : Window
    {
        private readonly AudioDeviceManager _deviceManager = new();
        private readonly AudioEngine _engine = new();
        private readonly SolidColorBrush _activePresetBrush = new(Color.FromRgb(126, 74, 255));
        private readonly SolidColorBrush _inactivePresetBrush = new(Color.FromRgb(47, 36, 64));

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            PitchSlider.ValueChanged += PitchSlider_ValueChanged;
            _engine.LevelUpdated += Engine_LevelUpdated;
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

            SetPreset(PresetMode.Clean);
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

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && Enum.TryParse<PresetMode>(button.Tag?.ToString(), out var preset))
            {
                SetPreset(preset);
            }
        }

        private void SetPreset(PresetMode preset)
        {
            _engine.SetPreset(preset);
            UpdatePresetVisuals(preset);
        }

        private void UpdatePresetVisuals(PresetMode active)
        {
            HighlightPresetButton(CleanButton, active == PresetMode.Clean);
            HighlightPresetButton(DemonButton, active == PresetMode.Demon);
            HighlightPresetButton(RobotButton, active == PresetMode.Robot);
            HighlightPresetButton(RadioButton, active == PresetMode.Radio);
        }

        private void HighlightPresetButton(Button button, bool isActive)
        {
            button.Background = isActive ? _activePresetBrush : _inactivePresetBrush;
            button.Opacity = isActive ? 1 : 0.8;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _engine.Dispose();
        }
    }
}
