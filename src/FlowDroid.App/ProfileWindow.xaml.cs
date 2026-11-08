using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid;

public partial class ProfileWindow : Window
{
    private readonly EmulatorProfile _profile;
    private readonly ProfileStore _store = new();
    private readonly ObservableCollection<ApkEntry> _apks;

    public ProfileWindow(EmulatorProfile profile)
    {
        InitializeComponent();
        _profile = profile;
        _profile.Apks ??= new List<ApkEntry>();
        _apks = new ObservableCollection<ApkEntry>(_profile.Apks);
        ApkList.ItemsSource = _apks;
        ProfileTitle.Text = _profile.Name;
        RefreshState();
    }

    private void AddApk_Click(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog { Title = "Выбери APK-файлы", Filter = "Android package (*.apk)|*.apk", Multiselect = true };
        if (picker.ShowDialog(this) != true) return;

        var directory = _store.GetProfileDirectory(_profile.Id);
        Directory.CreateDirectory(directory);

        foreach (var source in picker.FileNames)
        {
            var name = Path.GetFileName(source);
            if (_apks.Any(x => string.Equals(x.FileName, name, StringComparison.OrdinalIgnoreCase))) continue;
            var destination = Path.Combine(directory, $"{Guid.NewGuid():N}_{name}");
            try
            {
                File.Copy(source, destination);
                _apks.Add(new ApkEntry { FileName = name, StoredPath = destination, SizeBytes = new FileInfo(destination).Length });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось импортировать {name}.\n{ex.Message}", "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        SaveProfile();
        RefreshState();
    }

    private void RemoveApk_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ApkEntry entry }) return;
        if (MessageBox.Show($"Удалить {entry.FileName} из профиля и удалить импортированный файл?", "Удаление APK", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _apks.Remove(entry);
        try { if (File.Exists(entry.StoredPath)) File.Delete(entry.StoredPath); }
        catch (Exception ex) { MessageBox.Show($"Не удалось удалить файл.\n{ex.Message}", "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning); }
        SaveProfile();
        RefreshState();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileSettingsWindow(_profile) { Owner = this };
        if (dialog.ShowDialog() == true) { SaveProfile(); RefreshState(); }
    }

    private void SaveProfile() { _profile.Apks = _apks.ToList(); _store.Update(_profile); }
    private void RefreshState()
    {
        EmptyApkState.Visibility = _apks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ProfileSubtitle.Text = $"{_profile.AndroidVersion} · {_profile.Resolution} · {_profile.RamGb} ГБ RAM";
    }
}