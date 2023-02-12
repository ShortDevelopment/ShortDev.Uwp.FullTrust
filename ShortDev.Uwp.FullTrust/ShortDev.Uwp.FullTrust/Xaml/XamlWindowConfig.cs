using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Foundation;

namespace ShortDev.Uwp.FullTrust.Xaml;

public class XamlConfig
{
    public XamlTheme Theme { get; set; } = XamlTheme.Light;
    public bool HasTransparentBackground { get; set; } = false;
}

public sealed class XamlWindowConfig : XamlConfig
{
    public static XamlWindowConfig Default
    {
        get
        {
            string windowTitle = Process.GetCurrentProcess().ProcessName;
            try
            {
                windowTitle = Package.Current?.DisplayName ?? windowTitle;
            }
            catch { }
            return new(windowTitle);
        }
    }

    public XamlWindowConfig(string title)
        => Title = title;

    public string Title { get; }
    public bool HasWin32Frame { get; set; } = true;
    public bool HasWin32TitleBar { get; set; } = true;
    public bool IsTopMost { get; set; } = false;
    public bool IsVisible { get; set; } = false;
    public Rect? Bounds { get; set; } = null;
}
