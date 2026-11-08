using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FlowDroid.Services;

public static class MotionService
{
    public static void Show(UIElement? element, int durationMs = 190, double fromY = 8)
    {
        if (element is null) return;
        element.Visibility = Visibility.Visible;
        if (!ThemeService.Current.AnimationsEnabled)
        {
            element.Opacity = 1;
            element.RenderTransform = Transform.Identity;
            return;
        }

        element.BeginAnimation(UIElement.OpacityProperty, null);
        var transform = new TranslateTransform(0, fromY);
        element.RenderTransform = transform;
        element.Opacity = 0;

        var opacity = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var slide = new DoubleAnimation(fromY, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(UIElement.OpacityProperty, opacity);
        transform.BeginAnimation(TranslateTransform.YProperty, slide);
    }

    public static void Hide(UIElement? element, Action? completed = null, int durationMs = 130)
    {
        if (element is null) return;
        if (!ThemeService.Current.AnimationsEnabled)
        {
            element.Visibility = Visibility.Collapsed;
            completed?.Invoke();
            return;
        }

        var animation = new DoubleAnimation(element.Opacity, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        animation.Completed += (_, _) =>
        {
            element.Visibility = Visibility.Collapsed;
            element.Opacity = 1;
            completed?.Invoke();
        };
        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    public static void Pulse(UIElement? element)
    {
        if (element is null || !ThemeService.Current.AnimationsEnabled) return;
        var animation = new DoubleAnimation(0.985, 1, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.12 }
        };
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        element.RenderTransform = new ScaleTransform(0.985, 0.985);
        element.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        element.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }
}