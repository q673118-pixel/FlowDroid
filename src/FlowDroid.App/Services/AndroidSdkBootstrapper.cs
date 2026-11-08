using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;

namespace FlowDroid.Services;

public sealed class AndroidSdkBootstrapper
{
    private const string CommandLineToolsUrl =
        "https://dl.google.com/android/repository/commandlinetools-win-15859902_latest.zip";
    private const string JdkUrl =
        "https://aka.ms/download-jdk/microsoft-jdk-21-windows-x64.zip";
    private const string CommandLineToolsSha256 =
        "90AE805D20434428BFFCB699C290860F19BB5F66A67E6B330067E3DE801FB04A";

    private readonly string _root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FlowDroid", "AndroidSdk");

    public async Task<string> InstallAsync(
        string profileId, IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_root);
        var java = await EnsureJavaAsync(progress, cancellationToken);
        var sdkManager = await EnsureCommandLineToolsAsync(progress, cancellationToken);

        progress?.Report("Проверка лицензий Android SDK…");
        await RunBatchInteractiveAsync(sdkManager, new[] { $"--sdk_root={_root}", "--licenses" },
            java, TimeSpan.FromMinutes(10), cancellationToken);

        progress?.Report("Загрузка Android Emulator, ADB и системного образа Android 16…");
        await RunBatchAsync(sdkManager, new[]
        {
            $"--sdk_root={_root}",
            "--install",
            "platform-tools",
            "emulator",
            "build-tools;36.0.0",
            "platforms;android-36",
            "system-images;android-36;google_apis;x86_64"
        }, java, progress, TimeSpan.FromMinutes(120), cancellationToken);

        try
        {
            await EnsureScrcpyAsync(progress, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException)
        {
            progress?.Report("Android Runtime установлен. scrcpy не загрузился; приложения пока будут открываться в окне эмулятора.");
        }

        var avdManager = Path.Combine(_root, "cmdline-tools", "latest", "bin", "avdmanager.bat");
        var safeId = new string(profileId.Where(char.IsLetterOrDigit).Take(8).ToArray());
        if (string.IsNullOrWhiteSpace(safeId)) safeId = Guid.NewGuid().ToString("N")[..8];
        var avdName = "FlowDroid_" + safeId;

        var avdDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".android", "avd", avdName + ".avd");
        if (!Directory.Exists(avdDirectory))
        {
            progress?.Report("Создание отдельного виртуального устройства для профиля…");
            await RunBatchAsync(avdManager, new[]
            {
                "create", "avd", "--name", avdName,
                "--package", "system-images;android-36;google_apis;x86_64"
            }, java, progress, TimeSpan.FromMinutes(10), cancellationToken);
        }
        else
        {
            progress?.Report("Найдено существующее виртуальное устройство профиля — данные сохранены.");
        }

        Environment.SetEnvironmentVariable("ANDROID_HOME", _root, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", _root, EnvironmentVariableTarget.Process);
        progress?.Report("Android Runtime установлен. Профиль готов к запуску.");
        return avdName;
    }


    private static async Task EnsureScrcpyAsync(IProgress<string>? progress, CancellationToken token)
    {
        var target = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlowDroid", "Tools", "scrcpy");
        if (File.Exists(Path.Combine(target, "scrcpy.exe"))) return;

        progress?.Report("Загрузка scrcpy для отдельных окон Android-приложений…");
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("FlowDroid-Setup/0.3");
        using var response = await client.GetAsync(
            "https://api.github.com/repos/Genymobile/scrcpy/releases/latest", token);
        response.EnsureSuccessStatusCode();
        using var document = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(token));
        var asset = document.RootElement.GetProperty("assets").EnumerateArray()
            .FirstOrDefault(item =>
                item.GetProperty("name").GetString() is string name &&
                name.Contains("win64", StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
        if (asset.ValueKind == System.Text.Json.JsonValueKind.Undefined)
            throw new InvalidOperationException("Не найден официальный архив scrcpy для Windows x64.");

        var url = asset.GetProperty("browser_download_url").GetString()
            ?? throw new InvalidOperationException("У релиза scrcpy отсутствует ссылка на архив.");
        var archive = Path.Combine(Path.GetTempPath(), "flowdroid-scrcpy-win64.zip");
        await DownloadAsync(url, archive, progress, token);
        var extract = Path.Combine(Path.GetTempPath(), "flowdroid-scrcpy");
        if (Directory.Exists(extract)) Directory.Delete(extract, true);
        ZipFile.ExtractToDirectory(archive, extract, overwriteFiles: true);
        File.Delete(archive);

        var executable = Directory.EnumerateFiles(extract, "scrcpy.exe", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (executable is null)
            throw new InvalidOperationException("В архиве scrcpy не найден scrcpy.exe.");
        var sourceDirectory = Path.GetDirectoryName(executable)!;
        if (Directory.Exists(target)) Directory.Delete(target, true);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        Directory.Move(sourceDirectory, target);
        Directory.Delete(extract, true);
    }

    private async Task<string> EnsureJavaAsync(IProgress<string>? progress, CancellationToken token)
    {
        var javaOnPath = FindOnPath("java.exe");
        if (javaOnPath is not null)
        {
            var home = Directory.GetParent(Directory.GetParent(javaOnPath)!.FullName)!.FullName;
            return home;
        }

        var runtimeRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FlowDroid", "Runtime", "JDK21");
        var existing = Directory.Exists(runtimeRoot)
            ? Directory.EnumerateFiles(runtimeRoot, "java.exe", SearchOption.AllDirectories).FirstOrDefault()
            : null;
        if (existing is not null) return Directory.GetParent(Directory.GetParent(existing)!.FullName)!.FullName;

        progress?.Report("Загрузка Java 21 Runtime (нужен для Android SDK)…");
        var archive = Path.Combine(Path.GetTempPath(), "flowdroid-jdk21.zip");
        await DownloadAsync(JdkUrl, archive, progress, token);
        Directory.CreateDirectory(runtimeRoot);
        ZipFile.ExtractToDirectory(archive, runtimeRoot, overwriteFiles: true);
        File.Delete(archive);

        var java = Directory.EnumerateFiles(runtimeRoot, "java.exe", SearchOption.AllDirectories).FirstOrDefault();
        if (java is null) throw new InvalidOperationException("В загруженном Java Runtime не найден java.exe.");
        var javaHome = Directory.GetParent(Directory.GetParent(java)!.FullName)!.FullName;
        Environment.SetEnvironmentVariable("JAVA_HOME", javaHome, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("PATH",
            Path.Combine(javaHome, "bin") + Path.PathSeparator +
            Environment.GetEnvironmentVariable("PATH"), EnvironmentVariableTarget.Process);
        return javaHome;
    }

    private async Task<string> EnsureCommandLineToolsAsync(IProgress<string>? progress, CancellationToken token)
    {
        var bin = Path.Combine(_root, "cmdline-tools", "latest", "bin");
        var sdkManager = Path.Combine(bin, "sdkmanager.bat");
        if (File.Exists(sdkManager)) return sdkManager;

        progress?.Report("Загрузка официальных Android SDK Command-line Tools…");
        var archive = Path.Combine(Path.GetTempPath(), "flowdroid-commandlinetools.zip");
        var extract = Path.Combine(Path.GetTempPath(), "flowdroid-commandlinetools");
        if (Directory.Exists(extract)) Directory.Delete(extract, true);
        await DownloadAsync(CommandLineToolsUrl, archive, progress, token);
        using (var stream = File.OpenRead(archive))
        {
            var actualHash = Convert.ToHexString(SHA256.HashData(stream));
            if (!string.Equals(actualHash, CommandLineToolsSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Проверка SHA-256 Android SDK Command-line Tools не пройдена.");
        }
        ZipFile.ExtractToDirectory(archive, extract, overwriteFiles: true);
        File.Delete(archive);

        var source = Path.Combine(extract, "cmdline-tools");
        if (!Directory.Exists(source))
            throw new InvalidOperationException("Архив Android SDK Tools имеет неожиданную структуру.");
        Directory.CreateDirectory(Path.GetDirectoryName(bin)!);
        if (Directory.Exists(Path.Combine(_root, "cmdline-tools", "latest")))
            Directory.Delete(Path.Combine(_root, "cmdline-tools", "latest"), true);
        Directory.Move(source, Path.Combine(_root, "cmdline-tools", "latest"));
        Directory.Delete(extract, true);
        if (!File.Exists(sdkManager))
            throw new InvalidOperationException("Не удалось подготовить sdkmanager.bat.");
        return sdkManager;
    }

    private static async Task DownloadAsync(
        string url, string destination, IProgress<string>? progress, CancellationToken token)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        var total = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(token);
        await using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);
        var buffer = new byte[81920];
        long received = 0;
        int read;
        while ((read = await input.ReadAsync(buffer, token)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read), token);
            received += read;
            if (total is > 0)
                progress?.Report($"Загрузка компонентов… {received * 100 / total.Value}%");
        }
    }

    private static string BuildBatchCommand(string batchFile, IEnumerable<string> arguments)
    {
        var command = new StringBuilder().Append('"').Append(batchFile).Append('"');
        foreach (var argument in arguments)
            command.Append(' ').Append('"').Append(argument.Replace("\"", "\\\"")).Append('"');
        return command.ToString();
    }

    private static ProcessStartInfo CreateBatchStartInfo(string batchFile, IEnumerable<string> arguments, string javaHome)
    {
        var start = new ProcessStartInfo("cmd.exe")
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(batchFile)!,
            CreateNoWindow = false
        };
        start.ArgumentList.Add("/d");
        start.ArgumentList.Add("/c");
        start.ArgumentList.Add(BuildBatchCommand(batchFile, arguments));
        start.Environment["JAVA_HOME"] = javaHome;
        start.Environment["PATH"] = Path.Combine(javaHome, "bin") + Path.PathSeparator +
                                    Environment.GetEnvironmentVariable("PATH");
        return start;
    }

    private static async Task RunBatchInteractiveAsync(
        string batchFile, IEnumerable<string> arguments, string javaHome,
        TimeSpan timeout, CancellationToken token)
    {
        var start = CreateBatchStartInfo(batchFile, arguments, javaHome);
        using var process = Process.Start(start)
            ?? throw new InvalidOperationException("Не удалось открыть Android SDK License Manager.");
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeoutCts.CancelAfter(timeout);
        try { await process.WaitForExitAsync(timeoutCts.Token); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException("Проверка лицензий Android SDK превысила допустимое время.");
        }
        if (process.ExitCode != 0)
            throw new InvalidOperationException("Не удалось завершить принятие лицензий Android SDK.");
    }

    private static async Task RunBatchAsync(
        string batchFile, IEnumerable<string> arguments, string javaHome,
        IProgress<string>? progress, TimeSpan timeout, CancellationToken token)
    {
        var start = CreateBatchStartInfo(batchFile, arguments, javaHome);
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        using var process = new Process { StartInfo = start };
        var output = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) { output.AppendLine(e.Data); progress?.Report(e.Data); } };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) { output.AppendLine(e.Data); progress?.Report(e.Data); } };
        if (!process.Start()) throw new InvalidOperationException("Не удалось запустить Android SDK Manager.");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.StandardInput.WriteLineAsync("no");
        process.StandardInput.Close();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeoutCts.CancelAfter(timeout);
        try { await process.WaitForExitAsync(timeoutCts.Token); }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException("Установка Android Runtime превысила допустимое время.");
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException("Android SDK Manager завершился с ошибкой:\n" + output);
    }

    private static string? FindOnPath(string executable)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        return path?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(folder => Path.Combine(folder.Trim(), executable))
            .FirstOrDefault(File.Exists);
    }
}