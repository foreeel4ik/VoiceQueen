using System.ComponentModel;
using System.Windows;
using VoiceQueen.Views;

namespace VoiceQueen
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            AppState.Instance.PropertyChanged += OnAppStateChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NavigateAccordingToState();
        }

        private void OnAppStateChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.IsAuthenticated))
            {
                Dispatcher.Invoke(NavigateAccordingToState);
            }
        }

        private void NavigateAccordingToState()
        {
            if (AppState.Instance.IsAuthenticated)
            {
                if (RootFrame.Content is not DashboardPage)
                {
                    RootFrame.Navigate(new DashboardPage());
                }
            }
            else
            {
                if (RootFrame.Content is not LoginPage)
                {
                    RootFrame.Navigate(new LoginPage());
                }
            }
        }
    }
}
