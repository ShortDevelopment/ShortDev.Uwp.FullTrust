using System;
using System.IO;
using System.Reflection;
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
        //foreach (var winmdFile in Directory.GetFiles(AppContext.BaseDirectory, "*.winmd"))
        //    try
        //    {
        //        Assembly.LoadFrom(winmdFile);
        //    }
        //    catch { }
        // Assembly.LoadFrom(@"D:\Programmieren\Visual Studio Projects\ShortDev\ShortDev.Uwp.FullTrust\ShortDev.Uwp.Internal\lib\Windows.UI.Xaml.winmd");

        FullTrustApplication.Start((param) => new App(), new("Test") { HasTransparentBackground = true, IsVisible = true, HasWin32TitleBar = false });
    }
}
