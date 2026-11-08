namespace FlowDroid.Models;

public sealed class EmulatorProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Новый профиль";
    public string AndroidVersion { get; set; } = "Android 14";
    public int InstalledApkCount { get; set; }
    public string Resolution { get; set; } = "1920 × 1080";
    public string Status { get; set; } = "Не запускался";
}
