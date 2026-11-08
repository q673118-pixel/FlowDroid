using System.Windows;
using FlowDroid.Services;

namespace FlowDroid;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeService.Initialize();
        base.OnStartup(e);
    }
}
