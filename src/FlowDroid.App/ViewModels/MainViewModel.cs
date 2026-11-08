using FlowDroid.Infrastructure;
using FlowDroid.Services;

namespace FlowDroid.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    public MainViewModel(IDialogService dialogs)
    {
        Profiles = new ProfilesViewModel(new ProfileStore(), dialogs);
    }

    public ProfilesViewModel Profiles { get; }
}
