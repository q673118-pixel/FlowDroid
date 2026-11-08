# FlowDroid

FlowDroid is a Windows desktop project for managing Android profiles and launching user-provided APK applications.

## Current status

**Version:** 0.1.0 — early prototype.

Implemented:
- C# / WPF desktop shell targeting .NET 8 for Windows.
- Flat dark Material-inspired interface with a restrained purple accent.
- Create and persist profiles in `%LOCALAPPDATA%/FlowDroid/profiles.json`.
- Profile-specific APK import: selected APK files are copied into the profile's local storage folder.
- APK list and removal from a profile.
- Per-profile settings UI for resolution, DPI, orientation, RAM, CPU cores, target FPS and graphics renderer.
- GitHub Actions workflow for Windows x64 publishing.

Not implemented yet:
- Android virtualization/runtime integration.
- Actual APK installation into Android.
- Launching Android apps in separate Windows windows.
- Applying graphics/performance settings to a running Android runtime.
- APK package metadata parsing and icon extraction.

**Important:** importing an APK currently copies and catalogs the file. It does not install or run the APK. Those functions require the Android runtime integration phase.

## Technology

- C# / WPF
- .NET 8
- JSON profile persistence
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
