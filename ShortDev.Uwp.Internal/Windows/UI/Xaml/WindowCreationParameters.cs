using System.Runtime.InteropServices;

namespace Windows.UI.Xaml;

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
