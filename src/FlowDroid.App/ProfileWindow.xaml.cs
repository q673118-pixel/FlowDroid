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
    private AndroidSdkService _android = new();
    private readonly AndroidSdkBootstrapper _bootstrapper = new();
    private bool _loadingAvds;
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

    private async void ProfileWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshAvdsAsync();
    }

    private async void RefreshAvds_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAvdsAsync();
    }

    private async Task RefreshAvdsAsync()
    {
        _loadingAvds = true;
        AvdComboBox.Items.Clear();

        if (!_android.IsAvailable)
        {
            RuntimeStatusText.Text = "Android Runtime ещё не установлен. FlowDroid может загрузить необходимые компоненты автоматически — Android Studio не требуется.";
            AvdComboBox.IsEnabled = false;
            AutoSetupButton.IsEnabled = true;
            _loadingAvds = false;
            return;
        }

        try
        {
            var avds = await _android.GetAvdsAsync();
            foreach (var avd in avds) AvdComboBox.Items.Add(avd);

            if (!string.IsNullOrWhiteSpace(_profile.AndroidVirtualDevice) &&
                AvdComboBox.Items.Contains(_profile.AndroidVirtualDevice))
                AvdComboBox.SelectedItem = _profile.AndroidVirtualDevice;
            else if (AvdComboBox.Items.Count > 0)
                AvdComboBox.SelectedIndex = 0;

            AvdComboBox.IsEnabled = AvdComboBox.Items.Count > 0;
            RuntimeStatusText.Text = avds.Count == 0
                ? "SDK найден, но виртуальных устройств пока нет. Нажми «Настроить автоматически»."
                : $"Android Runtime готов · найдено устройств: {avds.Count}.";
            AutoSetupButton.IsEnabled = avds.Count == 0;
        }
        catch (Exception ex)
        {
            RuntimeStatusText.Text = "Не удалось получить список AVD.";
            MessageBox.Show(ex.Message, "Android Runtime", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            _loadingAvds = false;
        }
    }


    private async void AutoSetup_Click(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(
            "FlowDroid установит Android Runtime без Android Studio. Будут загружены Java 21, официальные Android SDK Command-line Tools, Emulator, ADB и образ Android 16 x86_64.\n\n" +
            "Для загрузки потребуется несколько гигабайт свободного места и стабильное подключение к интернету. Перед установкой компонентов нужно принять лицензии Android SDK.\n\nПродолжить?",
            "Автоматическая настройка Android Runtime",
            MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (answer != MessageBoxResult.Yes) return;

        AutoSetupButton.IsEnabled = false;
        StartAvdButton.IsEnabled = false;
        AvdComboBox.IsEnabled = false;
        RuntimeStatusText.Text = "Подготовка Android Runtime…";
        var progress = new Progress<string>(message => RuntimeStatusText.Text = message);

        try
        {
            var avdName = await _bootstrapper.InstallAsync(_profile.Id.ToString("N"), progress);
            _profile.AndroidVirtualDevice = avdName;
            _profile.AndroidVersion = "Android 16";
            _store.Update(_profile);
            _android = new AndroidSdkService();
            ProfileSubtitle.Text = $"{_profile.AndroidVersion} · {_profile.Resolution} · {_profile.RamGb} ГБ RAM";
            await RefreshAvdsAsync();
            RuntimeStatusText.Text = "Android Runtime установлен. Можно запускать виртуальную среду.";
        }
        catch (Exception ex)
        {
            RuntimeStatusText.Text = "Не удалось завершить автоматическую настройку.";
            MessageBox.Show(ex.Message, "Настройка Android Runtime", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            AutoSetupButton.IsEnabled = true;
            StartAvdButton.IsEnabled = true;
        }
    }

    private void AvdComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_loadingAvds || AvdComboBox.SelectedItem is not string avd) return;
        _profile.AndroidVirtualDevice = avd;
        _store.Update(_profile);
    }

    private async void StartAvd_Click(object sender, RoutedEventArgs e)
    {
        if (AvdComboBox.SelectedItem is not string avd)
        {
            MessageBox.Show("Сначала выбери виртуальное устройство или выполни автоматическую настройку Android Runtime.",
                "Android Runtime", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        StartAvdButton.IsEnabled = false;
        RuntimeStatusText.Text = "Запуск Android. Первая загрузка может занять несколько минут…";
        try
        {
            _profile.AndroidVirtualDevice = avd;
            _store.Update(_profile);
            await _android.StartAvdAsync(avd, _profile.RamGb, _profile.CpuCores, _profile.Resolution, _profile.Dpi, _profile.GraphicsRenderer, _profile.Orientation);
            RuntimeStatusText.Text = $"Android готов: {avd}. Применены параметры профиля. Теперь выбери APK и нажми «Установить и запустить».";
        }
        catch (Exception ex)
        {
            RuntimeStatusText.Text = "Не удалось запустить Android Runtime.";
            MessageBox.Show(ex.Message, "Android Runtime", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            StartAvdButton.IsEnabled = true;
        }
    }

    private async void InstallLaunch_Click(object sender, RoutedEventArgs e)
    {
        if (ApkList.SelectedItem is not ApkEntry selected)
        {
            MessageBox.Show("Сначала выбери APK в списке.", "FlowDroid", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RuntimeStatusText.Text = $"Установка {selected.FileName}…";
        try
        {
            var openedInSeparateWindow = await _android.InstallAndLaunchAsync(
                selected.StoredPath, _profile.Resolution, _profile.Dpi, _profile.TargetFps, _profile.Orientation);
            RuntimeStatusText.Text = openedInSeparateWindow
                ? selected.FileName + " открыт в отдельном окне FlowDroid."
                : selected.FileName + " запущен в окне Android Emulator. Для отдельного окна установи scrcpy.";
        }
        catch (Exception ex)
        {
            RuntimeStatusText.Text = "Не удалось установить или запустить APK.";
            MessageBox.Show(ex.Message, "Запуск APK", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
