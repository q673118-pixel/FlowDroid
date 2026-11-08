using System.Text.Json.Serialization;
using FlowDroid.Infrastructure;

namespace FlowDroid.Models;

public sealed class EmulatorProfile : ObservableObject
{
    private string _name = "Новый профиль";
    private string _androidVersion = "Android 16";
    private string _androidVirtualDevice = "";
    private string _resolution = "1920 × 1080";
    private int _dpi = 240;
    private string _orientation = "Альбомная";
    private int _ramGb = 4;
    private int _cpuCores = 4;
    private int _targetFps = 60;
    private string _graphicsRenderer = "Автоматически";
    private List<ApkEntry> _apks = new();
    private string _status = "Не запускался";

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get => _name; set => Set(ref _name, value); }
    public string AndroidVersion { get => _androidVersion; set => Set(ref _androidVersion, value); }
    public string AndroidVirtualDevice { get => _androidVirtualDevice; set => Set(ref _androidVirtualDevice, value); }
    public string Resolution { get => _resolution; set => Set(ref _resolution, value); }
    public int Dpi { get => _dpi; set => Set(ref _dpi, value); }
    public string Orientation { get => _orientation; set => Set(ref _orientation, value); }
    public int RamGb { get => _ramGb; set => Set(ref _ramGb, value); }
    public int CpuCores { get => _cpuCores; set => Set(ref _cpuCores, value); }
    public int TargetFps { get => _targetFps; set => Set(ref _targetFps, value); }
    public string GraphicsRenderer { get => _graphicsRenderer; set => Set(ref _graphicsRenderer, value); }
    public string Status { get => _status; set => Set(ref _status, value); }

    public List<ApkEntry> Apks
    {
        get => _apks;
        set
        {
            _apks = value ?? new List<ApkEntry>();
            OnPropertyChanged();
            OnPropertyChanged(nameof(InstalledApkCount));
        }
    }

    /// <summary>Number of imported APK files stored in this profile (derived, not persisted).</summary>
    [JsonIgnore]
    public int InstalledApkCount => _apks.Count;
}

public sealed class ApkEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FileName { get; set; } = "";
    public string StoredPath { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = "";
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
}
