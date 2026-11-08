using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FlowDroid.Services;

public static class WindowAppearance
{
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwcpRound = 2;

    public static void Apply(Window window)
    {
        window.SourceInitialized += (_, _) => ApplyNativeAppearance(window);
    }

    private static void ApplyNativeAppearance(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero) return;

        try
        {
            var preference = DwmwcpRound;
            DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference,
                ref preference, Marshal.SizeOf<int>());

            var darkMode = 1;
            DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode,
                ref darkMode, Marshal.SizeOf<int>());
        }
        catch (DllNotFoundException)
        {
            // Older Windows versions do not expose DWM attributes.
        }
        catch (EntryPointNotFoundException)
        {
            // Keep the app usable if the OS does not support this DWM API.
        }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd, int attribute, ref int value, int valueSize);
}