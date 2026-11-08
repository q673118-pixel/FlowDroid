using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<EmulatorProfile> _profiles = new();
    private readonly ProfileStore _profileStore = new();
    private ICollectionView? _profilesView;
    private bool _sortDescending;

    public MainWindow()
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        _profilesView = CollectionViewSource.GetDefaultView(_profiles);
        _profilesView.Filter = FilterProfile;
        ProfilesList.ItemsSource = _profilesView;
        LoadProfiles();
        UpdateProfileCount();
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        UpdateMaximizeButton();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        UpdateMaximizeButton();
    }

    private void UpdateMaximizeButton()
    {
        if (MaximizeButton is not null)
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
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

        if (_profiles.Any(profile => string.Equals(profile.Name, dialog.ProfileName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Профиль с таким названием уже существует.", "FlowDroid",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _profiles.Add(new EmulatorProfile { Name = dialog.ProfileName.Trim(), AndroidVersion = dialog.AndroidVersion });
        SaveProfiles();
        UpdateProfileCount();
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: EmulatorProfile profile }) return;
        if (MessageBox.Show(
                $"Удалить профиль «{profile.Name}» вместе со всеми импортированными APK-файлами?",
                "Удаление профиля", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            _profileStore.DeleteProfileData(profile.Id);
            _profiles.Remove(profile);
            SaveProfiles();
            UpdateProfileCount();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось полностью удалить данные профиля.\n{ex.Message}",
                "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

    private bool FilterProfile(object item)
    {
        if (item is not EmulatorProfile profile) return false;
        var query = ProfileSearchBox?.Text?.Trim();
        return string.IsNullOrEmpty(query)
            || profile.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
            || profile.AndroidVersion.Contains(query, StringComparison.CurrentCultureIgnoreCase);
    }

    private void ProfileSearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _profilesView?.Refresh();
        UpdateProfileCount();
    }

    private void SortProfiles_Click(object sender, RoutedEventArgs e)
    {
        _sortDescending = !_sortDescending;
        if (_profilesView is null) return;
        using (_profilesView.DeferRefresh())
        {
            _profilesView.SortDescriptions.Clear();
            _profilesView.SortDescriptions.Add(new SortDescription(
                nameof(EmulatorProfile.Name),
                _sortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        }
    }

    private void UpdateProfileCount()
    {
        var total = _profiles.Count;
        var visible = _profilesView?.Cast<object>().Count() ?? total;
        ProfileCountText.Text = total switch
        {
            1 => "1 профиль",
            >= 2 and <= 4 => $"{total} профиля",
            _ => $"{total} профилей"
        };

        var hasQuery = !string.IsNullOrWhiteSpace(ProfileSearchBox?.Text);
        var isEmpty = total == 0 && !hasQuery;
        var noResults = total > 0 && visible == 0 && hasQuery;

        ProfilesList.Visibility = visible > 0 ? Visibility.Visible : Visibility.Collapsed;
        CreateProfileTile.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        EmptyState.Visibility = isEmpty ? Visibility.Visible : Visibility.Collapsed;
        NoSearchResults.Visibility = noResults ? Visibility.Visible : Visibility.Collapsed;
    }
}
