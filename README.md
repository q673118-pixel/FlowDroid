# FlowDroid

**FlowDroid** is a Windows desktop project for managing isolated Android profiles and launching user-provided APK applications.

The product direction is app-first: users manage profiles and APKs in FlowDroid, then launch selected Android apps in their own windows. The Android runtime remains an implementation layer rather than the main user-facing workspace.

## Current status

**Version:** 0.1.0 — foundation / prototype

Implemented in this initial scaffold:
- WPF desktop application targeting .NET 8 for Windows.
- Flat, dark Material-inspired interface with a restrained purple accent, based on the MathFlow visual direction.
- Profile list and create-profile dialog (currently in-memory prototype data).
- GitHub Actions workflow for Windows x64 builds.

Not implemented yet:
- Android virtualization/runtime integration.
- Persistent profile storage.
- APK import, installation, and management.
- App-window launch and lifecycle management.
- Per-profile display, graphics, input, and performance settings.

## Technology

- C# / WPF
- .NET 8
- Android runtime integration to be selected and validated during the engine phase
- ADB for Android app installation and control, where supported
- GitHub Actions for CI builds

## Build locally

Requirements:
- Windows 10/11
- .NET 8 SDK

```powershell
dotnet restore FlowDroid.sln
dotnet build FlowDroid.sln --configuration Release
dotnet run --project src/FlowDroid.App/FlowDroid.App.csproj
```

## Build artifact

Pushes and pull requests to main trigger the Windows build workflow. The published output is uploaded as a GitHub Actions artifact named FlowDroid-win-x64.

## Product principles

- Profiles are isolated Android environments, not just folders of APK files.
- Users import their own APK files.
- Apps should launch directly from FlowDroid without requiring users to navigate an Android home screen.
- Performance, compatibility, and stability take priority over decorative effects.
- The interface uses solid surfaces, clear hierarchy, rounded controls, and purple accents; no glass effects, glow, or gratuitous gradients.

## License

No license has been selected yet. Until a license is added, the source remains subject to the default copyright rules.
