using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using FlowDroid.Models;
using FlowDroid.Services;

namespace FlowDroid;

public sealed class ManagedFileItem
{
    public required ApkEntry Apk { get; init; }
    public required string ProfileName { get; init; }
    public string FileName => Apk.FileName;
    public string Details => $"{ProfileName} · {Apk.SizeBytes / (1024d * 1024d):N1} МБ · {Apk.ImportedAtUtc.ToLocalTime():g}";
}

public partial class FilesWindow : Window
{
    private readonly ProfileStore _store = new();
    private readonly AppSettingsStore _settings = new();
    private readonly ObservableCollection<ManagedFileItem> _items = new();

    public FilesWindow()
    {
        InitializeComponent();
        WindowAppearance.Apply(this);
        FilesList.ItemsSource = _items;
        StoragePathText.Text = _settings.DataDirectory;
        LoadFiles();
        Loaded += (_, _) => MotionService.Show(EmptyState.Visibility == Visibility.Visible ? (UIElement)EmptyState : FilesList, 190, 8);
    }

    private void LoadFiles()
    {
        _items.Clear();
        foreach (var profile in _store.Load())
            foreach (var apk in profile.Apks ?? new List<ApkEntry>())
                if (File.Exists(apk.StoredPath))
                    _items.Add(new ManagedFileItem { Apk = apk, ProfileName = profile.Name });
        FileCountText.Text = $"{_items.Count} файлов";
        EmptyState.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OpenDataFolder_Click(object sender, RoutedEventArgs e) => OpenPath(_settings.DataDirectory);

    private void ShowInFolder_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ManagedFileItem item } && File.Exists(item.Apk.StoredPath))
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{item.Apk.StoredPath}\"") { UseShellExecute = true });
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: ManagedFileItem item } && File.Exists(item.Apk.StoredPath))
            Process.Start(new ProcessStartInfo(item.Apk.StoredPath) { UseShellExecute = true });
    }

    private static void OpenPath(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}