# FlowDroid

FlowDroid is a Windows desktop application for managing Android profiles and opening user-provided APKs in app-first Windows windows.

## Current status

**Version:** 0.3.0 — rounded MathFlow-inspired desktop UI and first Android Emulator / ADB / scrcpy launch path.

Implemented:
- C# / WPF desktop application targeting .NET 8 for Windows.
- Unified dark Material 3 visual system aligned with MathFlow: near-black surfaces, restrained purple accent, rounded controls and clear typography.
- Rounded top-level windows on Windows 10 and native rounded corners on Windows 11.
- Custom dark title bars and window controls.
- Persistent profiles in `%LOCALAPPDATA%/FlowDroid/profiles.json`.
- Profile creation, duplicate-name protection and profile deletion.
- Separate per-profile APK storage, multi-select import and SHA-256 duplicate detection.
- APK archive validation and file-size display.
- Per-profile resolution, DPI, orientation, RAM, CPU, FPS and renderer preferences.
- Android SDK discovery through `ANDROID_SDK_ROOT`, `ANDROID_HOME` and the standard Windows SDK directory.
- Android Virtual Device discovery and launch through Android Emulator.
- ADB boot readiness checks and APK installation.
- APK package-name extraction using Android SDK Build-Tools (`aapt`).
- App launch through ADB; optional scrcpy virtual-display windows when a compatible scrcpy installation is detected.
- Windows x64 self-contained publishing and Inno Setup installer generation through GitHub Actions.

Not implemented yet:
- Automatic download and installation of Android SDK components or scrcpy.
- Guaranteed one-AVD-per-profile lifecycle and full isolation of simultaneously running profiles.
- Applying every saved RAM / CPU / graphics-renderer preference to the Android Emulator process.
- Native WPF hosting of Android app surfaces; separate app windows are currently provided by scrcpy.
- APK icon extraction, rich package metadata, app permissions and version display.
- Hardware benchmarking and automatic resource recommendations.

## Android runtime setup

FlowDroid uses the official Android SDK tools already installed on the computer. Install these components through Android Studio's SDK Manager:
- Android SDK Platform-Tools (ADB)
- Android Emulator
- Android SDK Build-Tools (includes `aapt`)
- At least one Android Virtual Device (AVD)

FlowDroid detects the SDK from `ANDROID_SDK_ROOT`, `ANDROID_HOME` or the default Windows SDK directory under the user's local application data.

To open each APK in its own Windows window, install a compatible Windows build of [scrcpy from its official repository](https://github.com/Genymobile/scrcpy). Extract the complete scrcpy folder to:

`%LOCALAPPDATA%\FlowDroid\Tools\scrcpy\`

The folder must contain `scrcpy.exe` and its companion files. FlowDroid checks for the `--new-display` and `--start-app` options before using the separate-window mode.

If scrcpy is not available, FlowDroid installs and launches the APK inside the Android Emulator window instead.

## Launch an APK

1. Create a FlowDroid profile.
2. Open the profile and choose an existing AVD.
3. Start the Android environment and wait until it finishes booting.
4. Import one or more APK files.
5. Select an APK and choose **Install and launch**.

The first Android boot can take several minutes. Android Emulator acceleration depends on the host computer's virtualization configuration.

## Windows installer

Every successful push to `main` builds two downloadable artifacts in GitHub Actions:

- `FlowDroid-Setup` — Windows x64 installer (`FlowDroid-Setup-0.3.0.exe`). Installs per-user under `%LOCALAPPDATA%\Programs\FlowDroid`, creates a Start menu shortcut, optionally creates a desktop shortcut, and registers an uninstaller.
- `FlowDroid-win-x64` — portable self-contained application build.

The installer includes Russian and English UI and does not require administrator rights. Uninstalling FlowDroid does not intentionally remove profile data stored in `%LOCALAPPDATA%\FlowDroid`.

## Build locally

Requirements: Windows 10/11, .NET 8 SDK and Inno Setup 6.

```powershell
dotnet restore FlowDroid.sln
dotnet build FlowDroid.sln --configuration Release
dotnet run --project src/FlowDroid.App/FlowDroid.App.csproj
dotnet publish src/FlowDroid.App/FlowDroid.App.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true --output artifacts/FlowDroid
iscc installer/FlowDroid.iss
```

## Product principles

- Profiles represent Android environments, not just folders of APKs.
- Users import their own APKs.
- The target experience is app-first: launch selected apps directly in separate Windows windows where scrcpy virtual displays are supported.
- Performance, compatibility and stability take priority over decoration.
- Visual language: solid surfaces, rounded controls, restrained purple accents. No glass effects, glow or gratuitous gradients.

## License

No license has been selected. Until one is added, the repository remains subject to default copyright rules.
