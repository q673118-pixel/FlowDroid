using System.Collections.ObjectModel;
using System.Windows;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<EmulatorProfile> _profiles = new();
    private readonly ProfileStore _profileStore = new();

    public MainWindow()
    {
        InitializeComponent();
        ProfilesList.ItemsSource = _profiles;
        LoadProfiles();
        UpdateProfileCount();
    }

    private void LoadProfiles()
    {
        try
        {
            foreach (var profile in _profileStore.Load())
            {
                profile.Apks ??= new List<ApkEntry>();
                _profiles.Add(profile);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить профили.\n\n{ex.Message}", "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CreateProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateProfileWindow { Owner = this };
        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ProfileName)) return;
        _profiles.Add(new EmulatorProfile { Name = dialog.ProfileName.Trim(), AndroidVersion = dialog.AndroidVersion });
        SaveProfiles();
        UpdateProfileCount();
    }

    private void SaveProfiles()
    {
        try { _profileStore.Save(_profiles); }
        catch (Exception ex) { MessageBox.Show($"Не удалось сохранить профили.\n\n{ex.Message}", "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void OpenProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: EmulatorProfile profile }) return;
        var dialog = new ProfileWindow(profile) { Owner = this };
        dialog.ShowDialog();
        SaveProfiles();
        ProfilesList.Items.Refresh();
    }

    private void UpdateProfileCount()
    {
        var count = _profiles.Count;
        ProfileCountText.Text = count switch { 1 => "1 профиль", >= 2 and <= 4 => $"{count} профиля", _ => $"{count} профилей" };
        EmptyState.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
