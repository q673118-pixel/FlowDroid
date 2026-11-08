using System.Collections.ObjectModel;
using System.Windows;
using FlowDroid.Models;

namespace FlowDroid;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<EmulatorProfile> _profiles = new();

    public MainWindow()
    {
        InitializeComponent();
        ProfilesList.ItemsSource = _profiles;
        UpdateProfileCount();
    }

    private void CreateProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateProfileWindow { Owner = this };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ProfileName))
            return;

        _profiles.Add(new EmulatorProfile
        {
            Name = dialog.ProfileName.Trim(),
            AndroidVersion = dialog.AndroidVersion
        });
        UpdateProfileCount();
    }

    private void OpenProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: EmulatorProfile profile })
            return;

        MessageBox.Show(
            $"Профиль «{profile.Name}» создан. Управление APK и запуск Android-среды будут добавлены на следующих этапах.",
            "FlowDroid",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void UpdateProfileCount()
    {
        var count = _profiles.Count;
        ProfileCountText.Text = count switch
        {
            1 => "1 профиль",
            >= 2 and <= 4 => $"{count} профиля",
            _ => $"{count} профилей"
        };
    }
}
