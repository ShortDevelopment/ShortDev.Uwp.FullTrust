using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Types;

[StructLayout(LayoutKind.Sequential)]
public struct WindowCreationParameters
{
    public uint Left;
    public uint Top;
    public uint Width;
    public uint Height;
    [MarshalAs(UnmanagedType.I1)]
    public bool TransparentBackground;
    [MarshalAs(UnmanagedType.I1)]
    public bool IsCoreNavigationClient;
}
