using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TrialWorld.Presentation.Dialogs;

public partial class LoadPresetDialog : Window
{
    public string PresetName { get; private set; } = string.Empty;

    public LoadPresetDialog()
    {
        InitializeComponent();
        LoadButton.IsEnabled = false;
    }

    public void SetPresets(IEnumerable<string> presetNames)
    {
        PresetsListBox.ItemsSource = presetNames;
    }

    private void PresetsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadButton.IsEnabled = PresetsListBox.SelectedItem != null;
    }

    private void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        PresetName = PresetsListBox.SelectedItem as string ?? string.Empty;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}