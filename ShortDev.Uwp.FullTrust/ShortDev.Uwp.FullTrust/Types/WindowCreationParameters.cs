using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCreationParameters
    {
        public uint Left;
        public uint Top;
        public uint Width;
        public uint Height;
        public bool TransparentBackground;
        public bool IsCoreNavigationClient;
    }
}
