using System.IO;
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

    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowDroid");
    private string FilePath => Path.Combine(_directory, "profiles.json");

    public IReadOnlyList<EmulatorProfile> Load()
    {
        if (!File.Exists(FilePath)) return Array.Empty<EmulatorProfile>();
        return JsonSerializer.Deserialize<List<EmulatorProfile>>(File.ReadAllText(FilePath), JsonOptions)
               ?? new List<EmulatorProfile>();
    }

    public void Save(IEnumerable<EmulatorProfile> profiles)
    {
        Directory.CreateDirectory(_directory);
        var temporaryPath = FilePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profiles, JsonOptions));
        File.Move(temporaryPath, FilePath, overwrite: true);
    }

    public string GetProfileDirectory(Guid profileId) =>
        Path.Combine(_directory, "Profiles", profileId.ToString("N"));

    public string GetApkDirectory(Guid profileId) =>
        Path.Combine(GetProfileDirectory(profileId), "Apks");

    public void DeleteProfileData(Guid profileId)
    {
        var profileDirectory = GetProfileDirectory(profileId);
        if (Directory.Exists(profileDirectory))
            Directory.Delete(profileDirectory, recursive: true);
    }

    public void Update(EmulatorProfile updated)
    {
        var profiles = Load().ToList();
        var index = profiles.FindIndex(x => x.Id == updated.Id);
        if (index < 0) profiles.Add(updated);
        else profiles[index] = updated;
        Save(profiles);
    }
}
