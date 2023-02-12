using ShortDev.Uwp.FullTrust.Activation;
using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Uwp.FullTrust.Internal;
using ShortDev.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
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
        => CreateNewWindowInternal(config, out _);

    public static XamlWindow Attach(IntPtr hwnd, XamlConfig config)
    {
        PrepareWindowInternal(Win32Window.FromHwnd(hwnd));
        return MountXamlInternal(config, hwnd, null);
    }

    internal static XamlWindow CreateNewWindowInternal(XamlWindowConfig config, out CoreApplicationView coreView)
    {
        CoreWindow coreWindow = CoreWindowActivator.CreateCoreWindow(CoreWindowActivator.CoreWindowType.NOT_IMMERSIVE, config.Title, config.Bounds);

        // Enable async / await
        SynchronizationContext.SetSynchronizationContext(new XamlSynchronizationContext(coreWindow));

        // Create CoreApplicationView
        var coreApplicationPrivate = InteropHelper.RoGetActivationFactory<ICoreApplicationPrivate2>("Windows.ApplicationModel.Core.CoreApplication");
        Marshal.ThrowExceptionForHR(coreApplicationPrivate.CreateNonImmersiveView(out coreView));

        Win32Window win32Window = Win32Window.FromHwnd(coreWindow.GetHwnd());
        PrepareWindowInternal(win32Window);

        if (!config.IsVisible)
        {
            win32Window.IsCloaked = true;
            win32Window.ShowInTaskBar = false;
        }

        // Mount Xaml rendering
        var window = MountXamlInternal(config, win32Window.Hwnd, coreWindow);

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
    }

    static XamlWindow MountXamlInternal(XamlConfig config, IntPtr hwnd, CoreWindow? coreWindow)
    {
        var presenterStatic = InteropHelper.RoGetActivationFactory<IXamlPresenterStatics3>("Windows.UI.Xaml.Hosting.XamlPresenter");
        
        // Window will be created here (It attaches a subclass to CoreWindow)
        var presenter = coreWindow == null ? presenterStatic.CreateFromHwnd(hwnd) : presenterStatic.CreateFromCoreWindow(coreWindow);
        
        presenter.InitializePresenterWithTheme(config.Theme);

        // A XamlWindow inside a Win32 process is transparent by default
        // (See Windows.UI.Xaml.dll!DirectUI::DXamlCore::ConfigureCoreWindow)
        // This is to provide a consistent behavior across platforms
        presenter.TransparentBackground = config.HasTransparentBackground;

        // Get xaml window
        return XamlWindow.Current;
    }
}
