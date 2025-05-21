using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrialWorld.Presentation.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // TODO: Add main UI properties and commands
    }
}
