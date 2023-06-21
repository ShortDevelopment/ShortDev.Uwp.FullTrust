using Internal.Windows.ApplicationModel.Core;
using Internal.Windows.UI.Xaml;
using Internal.Windows.UI.Xaml.Hosting;
using ShortDev.Uwp.FullTrust.Activation;
using ShortDev.Uwp.FullTrust.Internal;
using ShortDev.Win32;
using System;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using WinUI.Interop.CoreWindow;

namespace ShortDev.Uwp.FullTrust.Xaml;

public static class XamlWindowActivator
{
    /// <summary>
    /// Creates new <see cref="XamlWindow"/> on current thread. <br />
    /// Only one window is allowed per thread! <br/>
    /// A <see cref="Win32WindowSubclass"/> will be attached automatically.
    /// </summary>
    public static XamlWindow CreateNewWindow(XamlWindowConfig config)
        => CreateNewWindowInternal(config, out _, out _);

    public static XamlWindow Attach(IntPtr hwnd, XamlConfig config)
    {
        PrepareWindowInternal(Win32Window.FromHwnd(hwnd));
        // var presenterStatic = nInteropHelper.RoGetActivationFactory<IXamlPresenterStatics3>("Windows.UI.Xaml.Hosting.XamlPresenter");

        // Window will be created here (It attaches a subclass to CoreWindow)
        var presenter = XamlPresenter.CreateFromHwnd((int)hwnd);
        presenter.InitializePresenterWithTheme(config.Theme);
        presenter.TransparentBackground = config.HasTransparentBackground;

        return Window.Current;
    }

    internal static CoreWindow PrepareNewCoreWindowInternal(XamlWindowConfig config, out CoreApplicationView coreView, out Win32Window win32Window)
    {
        var coreApplicationPrivate = InteropHelper.RoGetActivationFactory<ICoreApplicationPrivate2>("Windows.ApplicationModel.Core.CoreApplication");
        // Create dummy "CoreApplicationView" for "CoreWindow" constructor
        coreApplicationPrivate.CreateNonImmersiveView();

        // Create "CoreWindow"
        CoreWindow coreWindow = CoreWindowActivator.CreateCoreWindow(CoreWindowActivator.CoreWindowType.NOT_IMMERSIVE, config.Title, config.Bounds);

        // Create "CoreApplicationView"
        coreView = coreApplicationPrivate.CreateNonImmersiveView();

        // Create "TextInputProducer" to fix text input in "ContentDialog"
        // var textInputProducer = CoreWindowActivator.CreateTextInputProducer(coreWindow);

        // Enable async / await
        SynchronizationContext.SetSynchronizationContext(new XamlSynchronizationContext(coreWindow));

        win32Window = Win32Window.FromHwnd(coreWindow.GetHwnd());
        PrepareWindowInternal(win32Window);

        return coreWindow;
    }

    internal static XamlWindow CreateNewWindowInternal(XamlWindowConfig config, out CoreApplicationView coreView, out FrameworkView frameworkView)
    {
        var coreWindow = PrepareNewCoreWindowInternal(config, out coreView, out var win32Window);

        if (!config.IsVisible)
        {
            win32Window.IsCloaked = true;
            win32Window.ShowInTaskBar = false;
        }

        // Mount Xaml rendering
        frameworkView = new();
        frameworkView.Initialize(coreView);
        frameworkView.SetWindow(coreWindow);

        var window = Window.Current;

        // A XamlWindow inside a Win32 process is transparent by default
        // (See Windows.UI.Xaml.dll!DirectUI::DXamlCore::ConfigureCoreWindow)
        // This is to provide a consistent behavior across platforms
        ((IWindowPrivate)(object)window).TransparentBackground = config.HasTransparentBackground;
        // Application.Current.RequestedTheme = config.Theme;

        // Attach subclass to customize behavior of "CoreWindow"
        // Our subclass has to be attached after "FrameworkView"!
        Win32WindowSubclass subclass = Win32WindowSubclass.Attach(window);

        // Sync settings from "XamlWindowConfig"
        win32Window.HasWin32Frame = config.HasWin32Frame;
        win32Window.IsTopMost = config.IsTopMost;
        subclass.HasWin32TitleBar = config.HasWin32TitleBar;

        if (!config.IsVisible)
        {
            win32Window.Hide();
            win32Window.IsCloaked = false;
            win32Window.ShowInTaskBar = true;
        }
        else
        {
            // Show window
            coreWindow.Activate();
            win32Window.BringToFront();
        }

        return window;
    }

    static void PrepareWindowInternal(Win32Window window)
    {
        // Enable acrylic "HostBackdropBrush"
        window.EnableHostBackdropBrush();

        EnableMouseInPointer(true);
    }
}
