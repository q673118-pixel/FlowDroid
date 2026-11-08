# FlowDroid

**Current version: 0.5.1**

FlowDroid is a Windows desktop app for creating Android profiles and launching imported APKs in separate Windows windows.

## Version 0.5.1 — personalization and interface controls

### Implemented
- Russian and English interface language selection with persistent preferences and live translation of supported interface labels.
- Application-wide font selection with a live preview (Segoe UI Variable, Segoe UI, Arial, Calibri, Consolas and Georgia).
- Configurable transition speed and animated selection feedback in settings.
- Theme changes preserve language and typography preferences.
- Main window redesigned around the supplied FlowDroid reference: purple Android identity, fixed navigation rail, large hero heading, profile search, profile cards and dedicated empty state.
- Profile search and alphabetical sorting in the main view.
- Updated purple design tokens and lightweight hover motion for primary, secondary and navigation controls.
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

### Current verification status
The interface changes are committed to `main`. A successful Windows Release build has not yet been confirmed for these latest commits; do not treat the visual refresh as build-verified until the Windows Actions job completes.


## Appearance and motion system

The appearance work now includes:
- Dark, Light and Windows System theme modes.
- Six accent palettes: Purple, Ocean, Emerald, Sunset, Rose and Midnight.
- Persisted appearance preferences in the user's local application data.
- A dedicated appearance dialog reachable from the main-window title bar.
- A shared motion service for entrance, exit and subtle emphasis transitions.
- A reduced-motion option that disables the shared code-driven transitions and sets shared XAML motion durations to zero.
- Shared animation timing tokens for primary, secondary and navigation controls.
- Entrance transitions for the main content, profile cards, empty states and the appearance dialog.

The theme palette is applied through shared brushes so open views update when a theme is changed. System mode reads the current Windows app-theme preference when FlowDroid applies its theme.

### Verification note

The new appearance and motion files have been committed. A Windows Release build has not been run or confirmed in this environment; the repository's Windows Actions workflow remains the required validation step.


## Workspace pages (stages 5–6)

The main navigation now opens the following implemented workspaces:
- **Profiles:** profile dashboard and profile workspace with animated entry and APK empty-state transitions.
- **Applications:** searchable catalog aggregating imported APK files from all profiles; each item opens its owning profile.
- **Files:** lists managed APK files, shows their owning profile and import metadata, opens the containing file in Explorer, and opens the application data folder.
- **Settings:** persistent defaults for new profiles (RAM, CPU cores, target FPS), delete-confirmation preference, appearance settings shortcut, and application data-folder access.
- **About:** application/version information.

Global preferences are stored separately at %LOCALAPPDATA%/FlowDroid/settings.json. Profile-specific data and imported APK files remain in the existing profile storage.

The Applications catalog only lists APKs that are still present on disk. The Files page exposes managed imported APKs; it does not claim to be a general-purpose Windows file manager.

### Verification note

XAML event-handler references and key page wiring were checked against the committed code. A local .NET build could not be run in this environment because the .NET SDK is unavailable, and no successful GitHub Actions status is available for the latest commit. Treat the Windows Release build as unverified until CI completes.


## Stages 7–8 — Android runtime and APK execution

### Stage 7: runtime lifecycle and profile isolation
- Runtime provisioning is managed by FlowDroid and does not require Android Studio.
- Each profile receives a separate AVD name derived from its profile ID.
- Reopening a profile reuses its already-running AVD instead of starting a duplicate instance.
- RAM, CPU core count and graphics mode are passed to the emulator process at startup.
- Resolution, density and orientation are applied through ADB after Android finishes booting.
- A profile can explicitly stop its emulator through ADB; profile data remains on disk.
- The SDK bootstrapper validates the downloaded Android Command-line Tools archive with SHA-256.

### Stage 8: APK installation and app-window launch
- Imported APKs are checked as ZIP packages and must contain AndroidManifest.xml.
- SHA-256 is used to detect duplicate imports within a profile.
- ADB installs the selected APK into the profile's running Android instance.
- FlowDroid attempts to open the app in a dedicated scrcpy virtual-display window with the selected display size, DPI and target FPS.
- If a compatible scrcpy build is unavailable or its window fails to start, FlowDroid falls back to launching the app in the Android Emulator window.
- APK launch failures are surfaced to the user rather than being silently reported as success.
- The install-and-launch action prevents repeated clicks while an install is in progress.

### Runtime boundaries
FlowDroid is a Windows desktop controller around the official Android Emulator, ADB and scrcpy. Android apps are rendered by the Android runtime, not reimplemented as native WPF windows. The dedicated app window is provided by scrcpy's virtual display. Hardware compatibility, boot performance, app compatibility and the first-time SDK download still require testing on a real Windows machine.

## Stages 9–10 — diagnostics, reliability and release readiness

### Stage 9: Android Runtime diagnostics
- Added a dedicated diagnostics window to each profile.
- Reports Windows version and process architecture.
- Shows detected Android SDK, ADB, Emulator, Build-Tools and scrcpy paths.
- Queries connected Android devices through `adb devices -l`.
- Runs the Android Emulator acceleration check and includes actionable virtualization guidance when it fails.
- The report can be copied for troubleshooting.

### Stage 10: runtime reliability and performance
- If a newly started emulator fails to boot or startup is cancelled, FlowDroid attempts to terminate that emulator process tree instead of leaving a failed background instance.
- scrcpy virtual-display capability detection is cached per runtime service instance, avoiding a repeated `scrcpy --help` process launch for every APK.
- Updated application and installer version to 0.5.0.
- The Windows CI workflow continues to produce both the portable x64 build and per-user installer.

### Verification boundary
Diagnostics and code-level reliability improvements are implemented. They do not replace a real-device test of Android boot, APK install, scrcpy windows, and shutdown on a Windows 10/11 host. Check the latest GitHub Actions run before distributing the 0.5.0 installer.