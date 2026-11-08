using System.Windows;
using FlowDroid.Services;

namespace FlowDroid;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeService.Initialize();
        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, _) =>
            {
                if (sender is Window window) LocalizationService.Apply(window);
            }),
            true);
        base.OnStartup(e);
    }
}
