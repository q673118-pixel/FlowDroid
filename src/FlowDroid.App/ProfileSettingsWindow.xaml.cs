using System.Windows;
using System.Windows.Controls;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid;

public partial class ProfileSettingsWindow : Window
{
    private readonly EmulatorProfile _profile;

    public ProfileSettingsWindow(EmulatorProfile profile)
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        _profile = profile;

        Select(ResolutionBox, profile.Resolution);
        CustomResolutionBox.Text = profile.Resolution;
        Select(DpiBox, profile.Dpi.ToString());
        Select(OrientationBox, profile.Orientation);
        Select(RamBox, profile.RamGb.ToString());
        Select(CpuBox, profile.CpuCores.ToString());
        Select(FpsBox, profile.TargetFps.ToString());
        Select(RendererBox, profile.GraphicsRenderer);
        UpdateCustomResolutionVisibility();
    }

    private static void Select(ComboBox box, string value)
    {
        foreach (ComboBoxItem item in box.Items)
        {
            if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                box.SelectedItem = item;
                return;
            }
        }

        box.SelectedIndex = box.Items.Count - 1;
    }

    private static string Value(ComboBox box) =>
        (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

    private void ResolutionBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateCustomResolutionVisibility();

    private void UpdateCustomResolutionVisibility()
    {
        if (CustomResolutionBox is null || ResolutionBox is null) return;
        CustomResolutionBox.Visibility = Value(ResolutionBox) == "Пользовательское"
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var resolution = Value(ResolutionBox);
        if (resolution == "Пользовательское")
        {
            var normalized = CustomResolutionBox.Text.Trim().Replace('×', 'x').Replace(" ", "");
            var parts = normalized.Split('x', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out var width) ||
                !int.TryParse(parts[1], out var height) ||
                width < 320 || width > 7680 || height < 320 || height > 7680)
            {
                MessageBox.Show("Укажи корректное разрешение, например 1920x1080. Допустимый диапазон — от 320 до 7680 пикселей по каждой стороне.",
                    "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomResolutionBox.Focus();
                return;
            }

            resolution = $"{width} × {height}";
        }

        _profile.Resolution = resolution;
        _profile.Dpi = int.Parse(Value(DpiBox));
        _profile.Orientation = Value(OrientationBox);
        _profile.RamGb = int.Parse(Value(RamBox));
        _profile.CpuCores = int.Parse(Value(CpuBox));
        _profile.TargetFps = int.Parse(Value(FpsBox));
        _profile.GraphicsRenderer = Value(RendererBox);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
