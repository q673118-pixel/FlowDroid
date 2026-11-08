using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace FlowDroid.Services;

public sealed class AndroidSdkService
{
    public string? SdkPath { get; }
    public string? AdbPath { get; }
    public string? EmulatorPath { get; }
    public string? AaptPath { get; }
    public string? ScrcpyPath { get; }

    public bool IsAvailable => AdbPath is not null && EmulatorPath is not null;

    public AndroidSdkService()
    {
        ScrcpyPath = FindScrcpyPath();
        SdkPath = FindSdkPath();
        if (SdkPath is null) return;

        var adb = Path.Combine(SdkPath, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
        var emulator = Path.Combine(SdkPath, "emulator", OperatingSystem.IsWindows() ? "emulator.exe" : "emulator");
        AdbPath = File.Exists(adb) ? adb : null;
        EmulatorPath = File.Exists(emulator) ? emulator : null;

        var buildTools = Path.Combine(SdkPath, "build-tools");
        if (Directory.Exists(buildTools))
        {
            AaptPath = Directory.GetDirectories(buildTools)
                .OrderByDescending(path => ParseBuildToolsVersion(Path.GetFileName(path)))
                .Select(path => Path.Combine(path, OperatingSystem.IsWindows() ? "aapt.exe" : "aapt"))
                .FirstOrDefault(File.Exists);
        }
    }

    private static string? FindScrcpyPath()
    {
        var localTools = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlowDroid", "Tools", "scrcpy", "scrcpy.exe");
        if (File.Exists(localTools)) return localTools;

        var appTools = Path.Combine(AppContext.BaseDirectory, "Tools", "scrcpy", "scrcpy.exe");
        if (File.Exists(appTools)) return appTools;

        return Environment.GetEnvironmentVariable("PATH")?
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(folder => Path.Combine(folder.Trim(), "scrcpy.exe"))
            .FirstOrDefault(File.Exists);
    }

    private static string? FindSdkPath()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"),
            Environment.GetEnvironmentVariable("ANDROID_HOME"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk")
        };

        return candidates.Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetFullPath(path!))
            .FirstOrDefault(path => Directory.Exists(path) &&
                File.Exists(Path.Combine(path, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb")) &&
                File.Exists(Path.Combine(path, "emulator", OperatingSystem.IsWindows() ? "emulator.exe" : "emulator")));
    }

    private static Version ParseBuildToolsVersion(string? value) =>
        Version.TryParse(value, out var version) ? version : new Version(0, 0);

    public async Task<IReadOnlyList<string>> GetAvdsAsync(CancellationToken cancellationToken = default)
    {
        EnsureRuntime();
        var result = await RunAsync(EmulatorPath!, new[] { "-list-avds" }, TimeSpan.FromSeconds(15), cancellationToken);
        return result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public async Task StartAvdAsync(string avdName, CancellationToken cancellationToken = default)
    {
        EnsureRuntime();
        if (string.IsNullOrWhiteSpace(avdName)) throw new InvalidOperationException("Сначала выбери Android Virtual Device (AVD).");

        var start = new ProcessStartInfo(EmulatorPath!)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        };
        start.ArgumentList.Add("-avd");
        start.ArgumentList.Add(avdName);
        start.ArgumentList.Add("-no-snapshot-load");

        var process = Process.Start(start);
        if (process is null) throw new InvalidOperationException("Не удалось запустить Android Emulator.");

        await WaitForBootAsync(TimeSpan.FromMinutes(3), cancellationToken);
    }

    public async Task<string> WaitForBootAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        EnsureAdb();
        var deadline = DateTime.UtcNow + timeout;
        string? serial = null;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var devices = await RunAsync(AdbPath!, new[] { "devices" }, TimeSpan.FromSeconds(10), cancellationToken);
            serial = devices.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(line => line.Split('\t'))
                .Where(parts => parts.Length > 1 && parts[1] == "device" && parts[0].StartsWith("emulator-", StringComparison.OrdinalIgnoreCase))
                .Select(parts => parts[0])
                .FirstOrDefault();

            if (serial is not null)
            {
                var boot = await RunAsync(AdbPath!, new[] { "-s", serial, "shell", "getprop", "sys.boot_completed" },
                    TimeSpan.FromSeconds(10), cancellationToken);
                if (boot.StandardOutput.Trim() == "1") return serial;
            }

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        throw new TimeoutException("Android Emulator не завершил загрузку за отведённое время. Проверь виртуализацию и системный образ AVD.");
    }

    public async Task<bool> InstallAndLaunchAsync(
        string apkPath, string resolution, int dpi, int targetFps, string orientation,
        CancellationToken cancellationToken = default)
    {
        EnsureAdb();
        if (AaptPath is null)
            throw new InvalidOperationException("Не найден aapt.exe. Установи Android SDK Build-Tools через Android Studio SDK Manager.");

        if (!File.Exists(apkPath)) throw new FileNotFoundException("APK-файл не найден.", apkPath);

        var serial = await FindOnlineEmulatorAsync(cancellationToken);
        var metadata = await RunAsync(AaptPath, new[] { "dump", "badging", apkPath },
            TimeSpan.FromSeconds(30), cancellationToken);
        var match = Regex.Match(metadata.StandardOutput, @"^package: name='([^']+)'", RegexOptions.Multiline);
        if (!match.Success)
            throw new InvalidOperationException("Не удалось прочитать package name из APK. Проверь APK и версию Build-Tools.");

        var packageName = match.Groups[1].Value;
        var install = await RunAsync(AdbPath!, new[] { "-s", serial, "install", "-r", apkPath },
            TimeSpan.FromMinutes(2), cancellationToken);
        if (!install.StandardOutput.Contains("Success", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("ADB не подтвердил установку APK.\n" + install.StandardOutput + "\n" + install.StandardError);

        if (await TryStartScrcpyWindowAsync(serial, packageName, Path.GetFileNameWithoutExtension(apkPath),
                resolution, dpi, targetFps, orientation, cancellationToken))
            return true;

        await RunAsync(AdbPath!, new[] { "-s", serial, "shell", "monkey", "-p", packageName, "-c",
            "android.intent.category.LAUNCHER", "1" }, TimeSpan.FromSeconds(30), cancellationToken);
        return false;
    }

    private async Task<bool> TryStartScrcpyWindowAsync(
        string serial, string packageName, string title, string resolution, int dpi, int targetFps,
        string orientation, CancellationToken cancellationToken)
    {
        if (ScrcpyPath is null) return false;

        var help = await RunAsync(ScrcpyPath, new[] { "--help" }, TimeSpan.FromSeconds(15), cancellationToken);
        var helpText = help.StandardOutput + help.StandardError;
        if (!helpText.Contains("--new-display", StringComparison.Ordinal) ||
            !helpText.Contains("--start-app", StringComparison.Ordinal))
            return false;

        var match = Regex.Match(resolution, @"(\d+)\s*[×x]\s*(\d+)");
        var width = match.Success && int.TryParse(match.Groups[1].Value, out var parsedWidth) ? parsedWidth : 1280;
        var height = match.Success && int.TryParse(match.Groups[2].Value, out var parsedHeight) ? parsedHeight : 720;
        if (string.Equals(orientation, "Книжная", StringComparison.OrdinalIgnoreCase) && width > height)
            (width, height) = (height, width);

        var display = width + "x" + height + "/" + Math.Clamp(dpi, 120, 640);
        var start = new ProcessStartInfo(ScrcpyPath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(ScrcpyPath)!
        };
        var arguments = new[]
        {
            "--serial=" + serial,
            "--new-display=" + display,
            "--start-app=" + packageName,
            "--max-fps=" + Math.Clamp(targetFps, 15, 144),
            "--window-title=FlowDroid - " + title,
            "--no-vd-system-decorations",
            "--stay-awake"
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);

        var process = Process.Start(start);
        if (process is null) return false;
        process.Dispose();
        return true;
    }

    private async Task<string> FindOnlineEmulatorAsync(CancellationToken cancellationToken)
    {
        var result = await RunAsync(AdbPath!, new[] { "devices" }, TimeSpan.FromSeconds(10), cancellationToken);
        var serial = result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(line => line.Split('\t'))
            .Where(parts => parts.Length > 1 && parts[1] == "device" && parts[0].StartsWith("emulator-", StringComparison.OrdinalIgnoreCase))
            .Select(parts => parts[0])
            .FirstOrDefault();

        return serial ?? throw new InvalidOperationException("Запусти AVD и дождись полной загрузки Android.");
    }

    private void EnsureRuntime()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Android SDK Emulator или Platform-Tools не найдены. Установи Android Studio и Android SDK, затем проверь ANDROID_SDK_ROOT.");
    }

    private void EnsureAdb()
    {
        if (AdbPath is null)
            throw new InvalidOperationException("ADB не найден. Установи Android SDK Platform-Tools и проверь ANDROID_SDK_ROOT.");
    }

    private static async Task<ProcessResult> RunAsync(
        string executable, IEnumerable<string> arguments, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = start };
        if (!process.Start()) throw new InvalidOperationException($"Не удалось запустить {Path.GetFileName(executable)}.");

        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException($"{Path.GetFileName(executable)} не ответил за отведённое время.");
        }

        var output = await stdout;
        var error = await stderr;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{Path.GetFileName(executable)} завершился с ошибкой.\n{error}\n{output}");

        return new ProcessResult(output, error);
    }

    private sealed record ProcessResult(string StandardOutput, string StandardError);
}