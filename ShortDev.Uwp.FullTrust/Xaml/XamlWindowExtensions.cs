using ShortDev.Win32.Windowing;

namespace ShortDev.Uwp.FullTrust.Xaml;

public static class XamlWindowExtensions
{
    public static WindowSubclass GetSubclass(this XamlWindow window)
        => WindowSubclass.Attach(window.GetHwnd(), throwIfExists: false);
}
