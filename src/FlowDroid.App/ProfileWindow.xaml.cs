using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
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
        WindowAppearance.Apply(this);
        _profile = profile;
        _profile.Apks ??= new List<ApkEntry>();
        _apks = new ObservableCollection<ApkEntry>(_profile.Apks);
        ApkList.ItemsSource = _apks;
        ProfileTitle.Text = _profile.Name;
        RefreshState();
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        UpdateMaximizeButton();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();

    private void ProfileWindow_StateChanged(object? sender, EventArgs e) => UpdateMaximizeButton();

    private void UpdateMaximizeButton()
    {
        if (MaximizeButton is not null)
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    private void AddApk_Click(object sender, RoutedEventArgs e)
    {
        var picker = new OpenFileDialog
        {
            Title = "Выбери APK-файлы",
            Filter = "Android package (*.apk)|*.apk",
            Multiselect = true,
            CheckFileExists = true
        };
        if (picker.ShowDialog(this) != true) return;

        var directory = _store.GetApkDirectory(_profile.Id);
        Directory.CreateDirectory(directory);

        foreach (var source in picker.FileNames)
        {
            var name = Path.GetFileName(source);
            try
            {
                using (var archive = ZipFile.OpenRead(source))
                {
                    if (!archive.Entries.Any(entry =>
                        string.Equals(entry.FullName, "AndroidManifest.xml", StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"{name} не похож на корректный APK: AndroidManifest.xml не найден.",
                            "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }
                }

                string hash;
                using (var stream = File.OpenRead(source))
                    hash = Convert.ToHexString(SHA256.HashData(stream));

                if (_apks.Any(x => string.Equals(x.Sha256, hash, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var destination = Path.Combine(directory, $"{Guid.NewGuid():N}_{name}");
                File.Copy(source, destination);
                _apks.Add(new ApkEntry
                {
                    FileName = name,
                    StoredPath = destination,
                    SizeBytes = new FileInfo(destination).Length,
                    Sha256 = hash
                });
            }
            catch (InvalidDataException)
            {
                MessageBox.Show($"{name} не является корректным ZIP/APK-файлом.",
                    "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось импортировать {name}.\n{ex.Message}",
                    "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        SaveProfile();
        RefreshState();
    }

    private void RemoveApk_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ApkEntry entry }) return;
        if (MessageBox.Show($"Удалить {entry.FileName} из профиля и удалить импортированный файл?",
                "Удаление APK", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            if (File.Exists(entry.StoredPath)) File.Delete(entry.StoredPath);
            _apks.Remove(entry);
            SaveProfile();
            RefreshState();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось удалить файл.\n{ex.Message}",
                "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileSettingsWindow(_profile) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            SaveProfile();
            RefreshState();
        }
    }

    private void SaveProfile()
    {
        _profile.Apks = _apks.ToList();
        _store.Update(_profile);
    }

    private void RefreshState()
    {
        EmptyApkState.Visibility = _apks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ApkCountText.Text = _apks.Count switch { 1 => "1 APK", >= 2 and <= 4 => $"{_apks.Count} APK", _ => $"{_apks.Count} APK" };
        ProfileSubtitle.Text = $"{_profile.AndroidVersion} · {_profile.Resolution} · {_profile.RamGb} ГБ RAM";
    }
}
