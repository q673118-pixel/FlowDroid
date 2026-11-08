namespace FlowDroid.Models;

public sealed class EmulatorProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Новый профиль";
    public string AndroidVersion { get; set; } = "Android 14";
    public string AndroidVirtualDevice { get; set; } = "";
    public string Resolution { get; set; } = "1920 × 1080";
    public int Dpi { get; set; } = 240;
    public string Orientation { get; set; } = "Альбомная";
    public int RamGb { get; set; } = 4;
    public int CpuCores { get; set; } = 4;
    public int TargetFps { get; set; } = 60;
    public string GraphicsRenderer { get; set; } = "Автоматически";
    public List<ApkEntry> Apks { get; set; } = new();
    public int InstalledApkCount => Apks?.Count ?? 0;
    public string Status { get; set; } = "Не запускался";
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
