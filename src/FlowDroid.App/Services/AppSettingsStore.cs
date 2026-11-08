using System.IO;
using System.Text.Json;

namespace FlowDroid.Services;

public sealed class AppSettings
{
    public int DefaultRamGb { get; set; } = 4;
    public int DefaultCpuCores { get; set; } = 4;
    public int DefaultFps { get; set; } = 60;
    public bool ConfirmBeforeDelete { get; set; } = true;
}

public sealed class AppSettingsStore
{
    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowDroid");
    private string FilePath => Path.Combine(_directory, "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new AppSettings();
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings();
        }
        catch { return new AppSettings(); }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_directory);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, FilePath, true);
    }

    public string DataDirectory => _directory;
}