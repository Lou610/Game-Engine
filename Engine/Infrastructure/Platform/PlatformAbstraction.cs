using System;
using System.Runtime.InteropServices;

namespace Engine.Infrastructure.Platform;

/// <summary>
/// Platform-specific implementations abstraction
/// </summary>
public abstract class PlatformAbstraction
{
    public static PlatformAbstraction Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsPlatform();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOSPlatform();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxPlatform();
        }

        throw new PlatformNotSupportedException("Unsupported platform");
    }

    public abstract string GetPlatformName();
    public abstract IntPtr GetNativeWindowHandle();
}

internal class WindowsPlatform : PlatformAbstraction
{
    public override string GetPlatformName() => "Windows";
    public override IntPtr GetNativeWindowHandle() => IntPtr.Zero; // Implement with actual window handle
}

internal class MacOSPlatform : PlatformAbstraction
{
    public override string GetPlatformName() => "macOS";
    public override IntPtr GetNativeWindowHandle() => IntPtr.Zero; // Implement with actual window handle
}

internal class LinuxPlatform : PlatformAbstraction
{
    public override string GetPlatformName() => "Linux";
    public override IntPtr GetNativeWindowHandle() => IntPtr.Zero; // Implement with actual window handle
}

