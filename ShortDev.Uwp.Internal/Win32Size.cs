using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Size
    {
        public int X;
        public int Y;
    }
}
