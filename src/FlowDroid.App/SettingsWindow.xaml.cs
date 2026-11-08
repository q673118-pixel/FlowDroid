using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FlowDroid.Services;

namespace FlowDroid;

public partial class SettingsWindow : Window
{
    private readonly AppSettingsStore _store = new();
    private AppSettings _settings;

    public SettingsWindow()
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        _settings = _store.Load();
        Select(RamBox, _settings.DefaultRamGb);
        Select(CpuBox, _settings.DefaultCpuCores);
        Select(FpsBox, _settings.DefaultFps);
        ConfirmDeleteCheckBox.IsChecked = _settings.ConfirmBeforeDelete;
        SelectByTag(LanguageBox, ThemeService.Current.Language);
        SelectByTag(FontBox, ThemeService.Current.FontFamily);
        SelectByTag(MotionSpeedBox, ThemeService.Current.MotionSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture));
        FontPreviewText.FontFamily = new System.Windows.Media.FontFamily(ThemeService.Current.FontFamily);
        DataPathText.Text = _store.DataDirectory;
        Loaded += (_, _) => MotionService.Show(Content as UIElement, 190, 8);
    }

    private static void Select(ComboBox box, int value)
    {
        box.SelectedItem = box.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => int.TryParse(item.Content?.ToString(), out var n) && n == value)
            ?? box.Items.OfType<ComboBoxItem>().FirstOrDefault();
    }

    private static void SelectByTag(ComboBox combo, string value)
    {
        combo.SelectedItem = combo.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Tag?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            ?? combo.Items.OfType<ComboBoxItem>().FirstOrDefault();
    }

    private static string SelectedTag(ComboBox combo) =>
        (combo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;

    private void Personalization_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized || FontPreviewText is null) return;
        if (FontBox.SelectedItem is ComboBoxItem selected && selected.Tag is string family)
            FontPreviewText.FontFamily = new System.Windows.Media.FontFamily(family);
        MotionService.Pulse(FontPreviewCard);
    }

    private static int Value(ComboBox box, int fallback) =>
        int.TryParse((box.SelectedItem as ComboBoxItem)?.Content?.ToString(), out var n) ? n : fallback;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.DefaultRamGb = Value(RamBox, 4);
        _settings.DefaultCpuCores = Value(CpuBox, 4);
        _settings.DefaultFps = Value(FpsBox, 60);
        _settings.ConfirmBeforeDelete = ConfirmDeleteCheckBox.IsChecked == true;
        ThemeService.UpdatePreferences(
            SelectedTag(LanguageBox),
            SelectedTag(FontBox),
            double.TryParse(SelectedTag(MotionSpeedBox), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var speed) ? speed : 1.0);
        _store.Save(_settings);
        DialogResult = true;
    }

    private void Appearance_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AppearanceSettingsWindow { Owner = this };
        dialog.ShowDialog();
    }

    private void OpenDataFolder_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(_store.DataDirectory);
        Process.Start(new ProcessStartInfo(_store.DataDirectory) { UseShellExecute = true });
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}