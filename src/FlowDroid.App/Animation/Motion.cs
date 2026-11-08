using System.Windows;
using System.Windows.Media.Animation;

namespace FlowDroid.Animation;

/// <summary>
/// Central motion parameters for the whole application. Every animation in code uses these
/// values so durations and easing stay consistent. XAML templates use the same numbers.
/// </summary>
public static class Motion
{
    /// <summary>Set to false to disable animations (reduced motion).</summary>
    public static bool Enabled { get; set; } = true;

    /// <summary>Hover / press feedback.</summary>
    public static Duration Short => Make(140);

    /// <summary>Old section content leaving.</summary>
    public static Duration FadeOut => Make(110);

    /// <summary>Standard transitions: section content entering, cards.</summary>
    public static Duration Standard => Make(220);

    /// <summary>Sidebar indicator moving between items.</summary>
    public static Duration Navigation => Make(260);

    /// <summary>Complex appearances: dialogs, large surfaces.</summary>
    public static Duration Emphasized => Make(320);

    public static IEasingFunction EaseOut { get; } = Freeze(new CubicEase { EasingMode = EasingMode.EaseOut });
    public static IEasingFunction EaseIn { get; } = Freeze(new CubicEase { EasingMode = EasingMode.EaseIn });
    public static IEasingFunction EaseInOut { get; } = Freeze(new CubicEase { EasingMode = EasingMode.EaseInOut });

    /// <summary>Fast start with a soft landing - used for moving indicators and sheets.</summary>
    public static IEasingFunction Decelerate { get; } = Freeze(new QuarticEase { EasingMode = EasingMode.EaseOut });

    private static Duration Make(int milliseconds) =>
        new(TimeSpan.FromMilliseconds(Enabled ? milliseconds : 0));

    private static T Freeze<T>(T value) where T : Freezable
    {
        value.Freeze();
        return value;
    }
}
