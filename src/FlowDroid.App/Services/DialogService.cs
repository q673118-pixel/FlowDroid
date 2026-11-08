using System.Windows;
using FlowDroid.Models;

namespace FlowDroid.Services;

public sealed record CreateProfileResult(string Name, string AndroidVersion);

/// <summary>Abstraction over windows and message boxes so view models do not depend on WPF windows.</summary>
public interface IDialogService
{
    CreateProfileResult? ShowCreateProfile();
    void ShowProfile(EmulatorProfile profile);
    bool Confirm(string title, string message);
    void ShowMessage(string title, string message, bool isError = false);
}

public sealed class DialogService : IDialogService
{
    private static Window? Owner =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        ?? Application.Current?.MainWindow;

    public CreateProfileResult? ShowCreateProfile()
    {
        var dialog = new CreateProfileWindow { Owner = Owner };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ProfileName)) return null;
        return new CreateProfileResult(dialog.ProfileName.Trim(), dialog.AndroidVersion);
    }

    public void ShowProfile(EmulatorProfile profile)
    {
        var dialog = new ProfileWindow(profile) { Owner = Owner };
        dialog.ShowDialog();
    }

    public bool Confirm(string title, string message) =>
        Show(title, message, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

    public void ShowMessage(string title, string message, bool isError = false) =>
        Show(title, message, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);

    private static MessageBoxResult Show(string title, string message, MessageBoxButton buttons, MessageBoxImage image)
    {
        var owner = Owner;
        return owner is null
            ? MessageBox.Show(message, title, buttons, image)
            : MessageBox.Show(owner, message, title, buttons, image);
    }
}
