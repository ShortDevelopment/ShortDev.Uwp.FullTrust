using ShortDev.Uwp.FullTrust.Core.Interfaces;

namespace ShortDev.Uwp.FullTrust.Core.Activation
{
    public static class VirtualDesktopManagerActivator
    {
        public static IVirtualDesktopManager CreateVirtualDesktopManager()
            => InteropHelper.ComCreateInstance<IVirtualDesktopManager>("aa509086-5ca9-4c25-8f95-589d3c07b48a")!;
    }
}
