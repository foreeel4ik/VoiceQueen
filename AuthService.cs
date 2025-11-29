namespace VoiceQueen
{
    public class AuthService
    {
        private readonly AppState _state;

        public AuthService(AppState? state = null)
        {
            _state = state ?? AppState.Instance;
        }

        public bool TryLogin(string username, string password, bool rememberMe, out string message)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                message = "Username and password are required.";
                return false;
            }

            if (username == "admin" && password == "admin")
            {
                _state.SignIn(username, rememberMe);
                message = string.Empty;
                return true;
            }

            message = "Invalid username or password.";
            return false;
        }

        public void Logout()
        {
            _state.SignOut();
        }
    }
}
