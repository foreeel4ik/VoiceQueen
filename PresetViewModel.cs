using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VoiceQueen
{
    public class PresetViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Category { get; set; }
        public PresetMode Mode { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
