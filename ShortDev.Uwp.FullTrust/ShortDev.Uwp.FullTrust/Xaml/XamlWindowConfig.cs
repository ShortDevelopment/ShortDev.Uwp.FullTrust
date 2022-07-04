using System.Diagnostics;
using Windows.ApplicationModel;

namespace ShortDev.Uwp.FullTrust.Xaml
{
    public sealed class XamlWindowConfig
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
            => this.Title = title;

        public string Title { get; }
        public bool HasTransparentBackground { get; set; } = true;
        public bool HasWin32Frame { get; set; } = true;
        public bool HasWin32TitleBar { get; set; } = true;
        public bool IsTopMost { get; set; } = false;
        public bool IsVisible { get; set; } = true;
    }
}
