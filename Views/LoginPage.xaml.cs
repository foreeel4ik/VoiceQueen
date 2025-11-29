using System.Windows;
using System.Windows.Controls;
using VoiceQueen;

namespace VoiceQueen.Views
{
    public partial class LoginPage : Page
    {
        private readonly AuthService _authService = new();

        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text.Trim();
            var password = PasswordBox.Password;
            var remember = RememberCheck.IsChecked == true;

            if (_authService.TryLogin(username, password, remember, out var message))
            {
                ValidationText.Text = string.Empty;
                NavigationService?.Navigate(new DashboardPage());
            }
            else
            {
                ValidationText.Text = message;
            }
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }

        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ForgotPasswordPage());
        }
    }
}
