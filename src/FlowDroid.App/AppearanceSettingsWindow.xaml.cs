using System.Windows;
using System.Windows.Controls;
using FlowDroid.Services;

namespace FlowDroid;

public partial class AppearanceSettingsWindow : Window
{
    public AppearanceSettingsWindow()
    {
        InitializeComponent();
        SelectByTag(ThemeModeBox, ThemeService.Current.ThemeMode);
        SelectByTag(AccentBox, ThemeService.Current.Accent);
        AnimationsCheckBox.IsChecked = ThemeService.Current.AnimationsEnabled;
    }

    private static void SelectByTag(ComboBox combo, string value)
    {
        combo.SelectedItem = combo.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Tag?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            ?? combo.Items.OfType<ComboBoxItem>().FirstOrDefault();
    }

    private static string SelectedTag(ComboBox combo) =>
        (combo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Dark";

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        ThemeService.Update(
            SelectedTag(ThemeModeBox),
            SelectedTag(AccentBox),
            AnimationsCheckBox.IsChecked == true);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
}