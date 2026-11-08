using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace FlowDroid.Services;

public static class WindowAppearance
{
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwcpRound = 2;

    public static void Apply(Window window)
    {
        window.SourceInitialized += (_, _) => ApplyNativeAppearance(window);
        window.SizeChanged += (_, _) => ApplyLegacyRoundedRegion(window);
        window.StateChanged += (_, _) => ApplyLegacyRoundedRegion(window);
    }

    private static void ApplyNativeAppearance(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            try
            {
                var preference = DwmwcpRound;
                DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference,
                    ref preference, Marshal.SizeOf<int>());

                var darkMode = 1;
                DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode,
                    ref darkMode, Marshal.SizeOf<int>());
            }
            catch (DllNotFoundException) { }
            catch (EntryPointNotFoundException) { }
        }
        else
        {
            ApplyLegacyRoundedRegion(window);
        }
    }

    // Windows 10 has no DWM rounded-corner preference. A native window region
    // gives the same clipped silhouette without enabling WPF software transparency.
    private static void ApplyLegacyRoundedRegion(Window window)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)) return;
        if (!window.IsLoaded && PresentationSource.FromVisual(window) is null) return;

        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;

        var dpi = VisualTreeHelper.GetDpi(window);
        var width = Math.Max(1, (int)Math.Ceiling(window.ActualWidth * dpi.DpiScaleX));
        var height = Math.Max(1, (int)Math.Ceiling(window.ActualHeight * dpi.DpiScaleY));
        var radius = window.WindowState == WindowState.Maximized ? 0 : (int)Math.Round(18 * dpi.DpiScaleX);

        var region = CreateRoundRectRgn(0, 0, width + 1, height + 1, radius * 2, radius * 2);
        if (region == IntPtr.Zero) return;

        if (SetWindowRgn(handle, region, true) == 0)
            DeleteObject(region);
        // On success, Windows owns the region handle.
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int value, int valueSize);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateRoundRectRgn(
        int left, int top, int right, int bottom, int ellipseWidth, int ellipseHeight);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowRgn(IntPtr hwnd, IntPtr region, bool redraw);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr handle);
}