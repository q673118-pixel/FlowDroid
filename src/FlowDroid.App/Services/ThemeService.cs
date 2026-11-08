using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace FlowDroid.Services;

public static class ThemeService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FlowDroid", "appearance.json");

    private static AppearanceSettings _settings = new();

    public static AppearanceSettings Current => _settings;

    public static void Initialize()
    {
        try
        {
            if (File.Exists(SettingsPath))
                _settings = JsonSerializer.Deserialize<AppearanceSettings>(File.ReadAllText(SettingsPath)) ?? new();
        }
        catch
        {
            _settings = new AppearanceSettings();
        }

        Apply();
    }

    public static void Update(string mode, string accent, bool animationsEnabled)
    {
        _settings = new AppearanceSettings
        {
            ThemeMode = mode,
            Accent = accent,
            AnimationsEnabled = animationsEnabled
        };
        Apply();
        Save();
    }

    public static void Apply()
    {
        if (Application.Current is null) return;
        var resources = Application.Current.Resources;
        var light = _settings.ThemeMode switch
        {
            "Light" => true,
            "System" => IsWindowsLightTheme(),
            _ => false
        };

        var palette = GetAccent(_settings.Accent);
        SetColor(resources, "WindowColor", light ? "#F5F2F8" : "#0C0A10");
        SetColor(resources, "SurfaceColor", light ? "#FFFFFF" : "#17131F");
        SetColor(resources, "SurfaceVariantColor", light ? "#EEE8F5" : "#211A2D");
        SetColor(resources, "SurfaceElevatedColor", light ? "#E7DFF0" : "#2B213B");
        SetColor(resources, "TextColor", light ? "#21172B" : "#F3EFF8");
        SetColor(resources, "MutedTextColor", light ? "#6E6479" : "#A9A2B2");
        SetColor(resources, "OutlineColor", light ? "#D9D0E2" : "#30263D");
        SetColor(resources, "AccentColor", palette.Accent);
        SetColor(resources, "AccentHoverColor", palette.Hover);
        SetColor(resources, "AccentPressedColor", palette.Pressed);

        SetBrush(resources, "WindowBrush", light ? "#F5F2F8" : "#0C0A10");
        SetBrush(resources, "SurfaceBrush", light ? "#FFFFFF" : "#17131F");
        SetBrush(resources, "SurfaceVariantBrush", light ? "#EEE8F5" : "#211A2D");
        SetBrush(resources, "SurfaceElevatedBrush", light ? "#E7DFF0" : "#2B213B");
        SetBrush(resources, "TextBrush", light ? "#21172B" : "#F3EFF8");
        SetBrush(resources, "MutedTextBrush", light ? "#6E6479" : "#A9A2B2");
        SetBrush(resources, "OutlineBrush", light ? "#D9D0E2" : "#30263D");
        SetBrush(resources, "AccentBrush", palette.Accent);
        SetBrush(resources, "AccentHoverBrush", palette.Hover);
        SetBrush(resources, "AccentPressedBrush", palette.Pressed);
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* Appearance preferences must never prevent startup. */ }
    }

    private static bool IsWindowsLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return Convert.ToInt32(key?.GetValue("AppsUseLightTheme") ?? 0) == 1;
        }
        catch { return false; }
    }

    private static void SetColor(ResourceDictionary resources, string key, string hex)
    {
        resources[key] = (Color)ColorConverter.ConvertFromString(hex);
    }

    private static void SetBrush(ResourceDictionary resources, string key, string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        if (resources[key] is SolidColorBrush brush && !brush.IsFrozen)
            brush.Color = color;
        else
            resources[key] = new SolidColorBrush(color);
    }

    private static AccentPalette GetAccent(string name) => name switch
    {
        "Ocean" => new("#3188D8", "#4A9BE8", "#2165A8"),
        "Emerald" => new("#239B70", "#35B486", "#177653"),
        "Sunset" => new("#D66A43", "#E77F58", "#A94B2C"),
        "Rose" => new("#C64F83", "#DB6698", "#98375F"),
        "Midnight" => new("#5967C8", "#7482E2", "#3D4999"),
        _ => new("#7540C8", "#8752D8", "#542A9B")
    };

    private sealed record AccentPalette(string Accent, string Hover, string Pressed);
}

public sealed class AppearanceSettings
{
    public string ThemeMode { get; set; } = "Dark";
    public string Accent { get; set; } = "Purple";
    public bool AnimationsEnabled { get; set; } = true;
}