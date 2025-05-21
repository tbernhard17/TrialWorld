using System.Windows;

namespace TrialWorld.Presentation.Dialogs;

public partial class SavePresetDialog : Window
{
    public string PresetName { get; private set; } = string.Empty;

    public SavePresetDialog()
    {
        InitializeComponent();
        SaveButton.IsEnabled = false;
    }

    private void PresetNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(PresetNameTextBox.Text);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        PresetName = PresetNameTextBox.Text.Trim();
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}