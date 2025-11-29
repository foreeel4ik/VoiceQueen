using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            RootFrame.Navigated += RootFrame_Navigated;
            RootFrame.Navigating += RootFrame_Navigating;
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

        private void RootFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (RootFrame.Content is FrameworkElement element)
            {
                BeginFadeSlide(element, 1, 0.94, 0, 14, 180);
            }
        }

        private void RootFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content is FrameworkElement element)
            {
                element.Opacity = 0;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new TranslateTransform(0, 16);

                element.Loaded += (_, _) => BeginFadeSlide(element, 0, 1, 16, 0, 240);
            }
        }

        private static void BeginFadeSlide(FrameworkElement element, double fromOpacity, double toOpacity, double fromY, double toY, double durationMs)
        {
            var transform = element.RenderTransform as TranslateTransform ?? new TranslateTransform();
            element.RenderTransform = transform;

            var storyboard = new Storyboard
            {
                Duration = TimeSpan.FromMilliseconds(durationMs)
            };

            var fade = new DoubleAnimation(fromOpacity, toOpacity, storyboard.Duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fade, element);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

            var slide = new DoubleAnimation(fromY, toY, storyboard.Duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slide, transform);
            Storyboard.SetTargetProperty(slide, new PropertyPath(TranslateTransform.YProperty));

            storyboard.Children.Add(fade);
            storyboard.Children.Add(slide);
            storyboard.Begin();
        }
    }
}
