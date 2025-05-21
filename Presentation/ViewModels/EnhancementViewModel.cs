using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrialWorld.Presentation.ViewModels
{
    // Stub view model to satisfy DI registration
    public class EnhancementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        // TODO: Add properties and commands
    }
}
