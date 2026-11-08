using System.Text.Json;
using FlowDroid.Models;

namespace FlowDroid.Services;

public sealed class ProfileStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _directory;
    private readonly string _filePath;

    public ProfileStore()
    {
        _directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlowDroid");
        _filePath = Path.Combine(_directory, "profiles.json");
    }

    public IReadOnlyList<EmulatorProfile> Load()
    {
        if (!File.Exists(_filePath))
            return Array.Empty<EmulatorProfile>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<EmulatorProfile>>(json, JsonOptions)
               ?? new List<EmulatorProfile>();
    }

    public void Save(IEnumerable<EmulatorProfile> profiles)
    {
        Directory.CreateDirectory(_directory);

        var temporaryPath = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(profiles, JsonOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _filePath, overwrite: true);
    }
}
