using System.Windows;
using System.Windows.Controls;
using FlowDroid.Models;

namespace FlowDroid;

public partial class ProfileSettingsWindow : Window
{
    private readonly EmulatorProfile _profile;
    public ProfileSettingsWindow(EmulatorProfile profile)
    {
        InitializeComponent();
        _profile = profile;
        Select(ResolutionBox, profile.Resolution); Select(DpiBox, profile.Dpi.ToString());
        Select(OrientationBox, profile.Orientation); Select(RamBox, profile.RamGb.ToString());
        Select(CpuBox, profile.CpuCores.ToString()); Select(FpsBox, profile.TargetFps.ToString());
        Select(RendererBox, profile.GraphicsRenderer);
    }
    private static void Select(ComboBox box, string value)
    {
        foreach (ComboBoxItem item in box.Items)
            if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase)) { box.SelectedItem = item; return; }
        box.SelectedIndex = 0;
    }
    private static string Value(ComboBox box) => (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _profile.Resolution = Value(ResolutionBox); _profile.Dpi = int.Parse(Value(DpiBox));
        _profile.Orientation = Value(OrientationBox); _profile.RamGb = int.Parse(Value(RamBox));
        _profile.CpuCores = int.Parse(Value(CpuBox)); _profile.TargetFps = int.Parse(Value(FpsBox));
        _profile.GraphicsRenderer = Value(RendererBox); DialogResult = true;
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}