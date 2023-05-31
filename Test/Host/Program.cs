using System;
using UwpUI;
using Windows.UI.Xaml;

namespace VBAudioRouter.Host;

static class Program
{

    [STAThread]
    static void Main()
    {
        // https://raw.githubusercontent.com/fboldewin/COM-Code-Helper/master/code/interfaces.txt
        // GOOGLE: "IApplicationViewCollection" site:lise.pnfsoftware.com

        FullTrustApplication.Start((param) => new App(), new("Test") { HasTransparentBackground = true, IsVisible = true, HasWin32TitleBar = false });
    }
}
