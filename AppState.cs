using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoiceQueen
{
    public class AppState : INotifyPropertyChanged
    {
        private static readonly Lazy<AppState> _instance = new(() => new AppState());
        public static AppState Instance => _instance.Value;

        private bool _isAuthenticated;
        private string? _username;
        private bool _rememberMe;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            private set => SetField(ref _isAuthenticated, value);
        }

        public string? Username
        {
            get => _username;
            private set => SetField(ref _username, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            private set => SetField(ref _rememberMe, value);
        }

        private AppState()
        {
        }

        public void SignIn(string username, bool rememberMe)
        {
            Username = username;
            RememberMe = rememberMe;
            IsAuthenticated = true;
        }

        public void SignOut()
        {
            Username = null;
            RememberMe = false;
            IsAuthenticated = false;
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
