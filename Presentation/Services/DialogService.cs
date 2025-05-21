using TrialWorld.Presentation.Interfaces;
using System.Threading.Tasks;
using System.Windows;

namespace TrialWorld.Presentation.Services;

public class DialogService : TrialWorld.Presentation.Interfaces.IDialogService
{
    public bool ShowConfirmation(string message, string title = "Confirmation")
    {
        // TODO: Implement confirmation dialog
        return false;
    }

    public void ShowError(string message, string title = "Error")
    {
        // TODO: Implement error dialog
    }

    public void ShowMessage(string message, string title = "Information")
    {
        // TODO: Implement message dialog
    }
} 