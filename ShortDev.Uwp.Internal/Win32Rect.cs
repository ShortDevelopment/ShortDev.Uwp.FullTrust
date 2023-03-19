using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
