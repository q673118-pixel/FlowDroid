using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using FlowDroid.Animation;
using FlowDroid.Services;
using FlowDroid.ViewModels;
using FlowDroid.Views;

namespace FlowDroid;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, UserControl> _views = new();
    private int _navigationToken;

    public MainWindow()
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        DataContext = new MainViewModel(new DialogService());
        Host.Content = GetView("home");
        StateChanged += (_, _) =>
        {
            var maximized = WindowState == WindowState.Maximized;
            Frame.Padding = maximized ? new Thickness(7) : new Thickness(0);
            Frame.CornerRadius = maximized ? new CornerRadius(0) : new CornerRadius(18);
        };
    }

    private void Nav_Navigated(object? sender, string key)
    {
        if (!IsLoaded) return;
        var next = GetView(key);
        if (ReferenceEquals(Host.Content, next)) return;
        _ = SwitchAsync(next);
    }

    // Old content fades out quickly, the new one fades in with a small upward shift.
    // The token makes a newer click win over a transition that is still running.
    private async Task SwitchAsync(UserControl next)
    {
        var token = ++_navigationToken;

        if (Motion.Enabled)
        {
            var fadeOut = new DoubleAnimation(0, Motion.FadeOut) { EasingFunction = Motion.EaseIn };
            Host.BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(Motion.FadeOut.TimeSpan);
            if (token != _navigationToken) return;
        }

        Host.Content = next;

        if (!Motion.Enabled)
        {
            Host.BeginAnimation(OpacityProperty, null);
            Host.Opacity = 1;
            return;
        }

        Host.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, Motion.Standard) { EasingFunction = Motion.EaseOut });
        HostShift.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty,
            new DoubleAnimation(14, 0, Motion.Standard) { EasingFunction = Motion.Decelerate });
    }

    private UserControl GetView(string key)
    {
        if (_views.TryGetValue(key, out var existing)) return existing;

        UserControl view = key switch
        {
            "home" => new HomeView(),
            "profiles" => new ProfilesView(),
            "apps" => Placeholder("Приложения",
                "Каталог импортированных APK и установка в профиль.",
                "Каталог приложений появится на следующем этапе."),
            "files" => Placeholder("Файлы",
                "Управление файлами профилей и занятым местом.",
                "Файловый раздел появится на следующем этапе."),
            "settings" => Placeholder("Настройки",
                "Тема, акцент, анимации и параметры Android.",
                "Настройки и темы появятся на этапах 3–4."),
            _ => Placeholder("О программе", "FlowDroid — Android-эмулятор для Windows.",
                "Сведения о версиях компонентов будут добавлены позже.")
        };

        view.DataContext = DataContext;
        _views[key] = view;
        return view;
    }

    private static SectionPlaceholderView Placeholder(string heading, string description, string note) =>
        new() { Heading = heading, Description = description, Note = note };
}
