# FlowDroid

FlowDroid is a Windows desktop app for creating Android profiles and launching imported APKs in separate Windows windows.

## Version 0.4.0

### Implemented
- Dark Material-inspired interface aligned with MathFlow: solid near-black surfaces, restrained purple accent, rounded cards and controls.
- Custom title bars, native Windows 11 rounded corners and rounded window regions on Windows 10.
- Custom dark ComboBox dropdowns, text fields, buttons, lists and popup surfaces (no white system dropdowns).
- Profile creation, persistent settings and isolated per-profile APK storage.
- APK import, ZIP/manifest validation, SHA-256 duplicate detection and removal.
- One-click Android Runtime provisioning without Android Studio:
  - Microsoft OpenJDK 21
  - Official Android SDK Command-line Tools
  - Android Emulator, ADB, Build-Tools and Android 16 x86_64 Google APIs system image
  - An AVD generated for each FlowDroid profile
  - Official scrcpy Windows x64 release for independent app windows
- Android Emulator startup with per-profile RAM, CPU cores and graphics mode.
- Android display size, density and orientation applied after boot.
- APK package metadata extraction, ADB install and app launch.
- scrcpy virtual display with per-app window title, selected resolution, DPI and target FPS.
- Self-contained Windows x64 publishing and Inno Setup installer.

### Automatic Android setup

Open a profile and choose **Настроить автоматически**. FlowDroid downloads the required components into:

`%LOCALAPPDATA%\FlowDroid\AndroidSdk`

The JDK is stored under:

`%LOCALAPPDATA%\FlowDroid\Runtime\JDK21`

scrcpy is stored under:

`%LOCALAPPDATA%\FlowDroid\Tools\scrcpy`

Android Studio is not required. The first setup is a large download (several GB); a stable internet connection and several GB of free disk space are required. Android SDK license terms are shown before setup and must be accepted. Hardware virtualization must be enabled for acceptable emulator performance.

### Launching APKs

1. Create a profile.
2. Open it and click **Настроить автоматически** if Android Runtime is not installed.
3. Wait for the Android SDK, system image, AVD and scrcpy setup to finish.
4. Click **Запустить среду** and wait for Android to boot.
5. Import an APK, select it and choose **Установить и запустить**.

When a compatible scrcpy virtual display is available, the app opens in its own Windows window. The Android Emulator remains the underlying runtime. This is not native WPF rendering of Android apps.

### Known limitations
- Android Emulator compatibility and speed depend on the host CPU, graphics drivers and enabled virtualization.
- The first Android boot and first component download can take several minutes.
- Android 16 Google APIs x86_64 is the default runtime image; some ARM-only apps or apps requiring Google Play certification may not work.
- Multiple profiles receive separate AVD definitions, but simultaneous heavy emulator sessions are limited by host resources.
- The profile graphics selector maps to Android Emulator's supported auto, host and SwiftShader modes; Vulkan is not exposed as a separate guaranteed backend.
- Some apps with DRM, anti-cheat, hardware attestation or device-specific requirements may refuse to run in an emulator.

## Build

Requirements: Windows 10/11 and .NET 8 SDK. The GitHub Actions workflow creates:
- `FlowDroid-Setup`: per-user Windows installer.
- `FlowDroid-win-x64`: portable self-contained build.

## Repository

https://github.com/q673118-pixel/FlowDroid
