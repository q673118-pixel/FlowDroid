# FlowDroid

FlowDroid is a Windows desktop application for managing isolated Android profiles and preparing user-provided APKs for app-first launching.

## Current status

**Version:** 0.1.1 — profile and settings prototype.

Implemented:
- C# / WPF desktop shell targeting .NET 8 for Windows.
- Flat dark Material-inspired interface with a restrained purple accent.
- Persistent profiles in `%LOCALAPPDATA%/FlowDroid/profiles.json`.
- Profile creation, duplicate-name protection and profile deletion.
- Separate per-profile APK storage.
- Multi-select APK import with ZIP/APK structure check.
- SHA-256 duplicate detection for imported APKs.
- APK list, file size display and removal.
- Per-profile display settings: preset or custom resolution, DPI and orientation.
- Per-profile performance settings: RAM, CPU core count and target FPS.
- Per-profile graphics renderer preference.
- GitHub Actions workflow for Windows x64 publishing.

Not implemented yet:
- Android virtualization/runtime integration.
- Actual APK installation into Android.
- Launching Android apps in separate Windows windows.
- Applying graphics/performance settings to a running Android runtime.
- APK package metadata parsing and icon extraction.
- Hardware capability detection and automatic resource recommendations.

**Important:** importing an APK currently validates its archive structure, copies it into the selected profile and catalogs it. It does not install or run the APK. Settings are persisted but are not applied to a runtime until the Android engine integration is implemented.

## Technology

- C# / WPF
- .NET 8
- JSON profile persistence
- SHA-256 for duplicate APK detection
- Android runtime and ADB integration to be selected and validated in a later phase
- GitHub Actions

## Build locally

Requirements: Windows 10/11 and .NET 8 SDK.

```powershell
dotnet restore FlowDroid.sln
dotnet build FlowDroid.sln --configuration Release
dotnet run --project src/FlowDroid.App/FlowDroid.App.csproj
```

## Product principles

- Profiles represent separate Android environments, not just folders of APKs.
- Users import their own APK files.
- The target experience is app-first: launch selected apps directly from FlowDroid, without requiring users to navigate an Android home screen.
- Performance, compatibility and stability take priority over decoration.
- Visual language: solid surfaces, clear hierarchy, rounded controls and purple accents. No glass effects, glow or gratuitous gradients.

## License

No license has been selected. Until one is added, the repository remains subject to default copyright rules.
