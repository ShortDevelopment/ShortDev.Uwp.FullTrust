using ShortDev.Uwp.FullTrust.Activation;
using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Win32;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinUI.Interop.CoreWindow;
using XamlFrameworkView = Windows.UI.Xaml.FrameworkView;
using XamlWindow = Windows.UI.Xaml.Window;

namespace ShortDev.Uwp.FullTrust.Xaml
{
    public sealed class XamlWindowActivator
    {
        /// <summary>
        /// Creates new <see cref="XamlWindow"/> on current thread. <br />
        /// Only one window is allowed per thread! <br/>
        /// A <see cref="XamlWindowSubclass"/> will be attached automatically.
        /// </summary>
        public static XamlWindow CreateNewWindow(XamlWindowConfig config)
            => CreateNewInternal(config).window;

        internal static (CoreApplicationView coreAppView, XamlWindow window) CreateNewInternal(XamlWindowConfig config)
        {
            CoreWindow coreWindow = CoreWindowActivator.CreateCoreWindow(CoreWindowActivator.WindowType.NOT_IMMERSIVE, config.Title, config.Bounds);

            // Enable async / await
            SynchronizationContext.SetSynchronizationContext(new XamlSynchronizationContext(coreWindow));

            // Create CoreApplicationView
            var coreApplicationPrivate = InteropHelper.RoGetActivationFactory<ICoreApplicationPrivate2>("Windows.ApplicationModel.Core.CoreApplication");
            Marshal.ThrowExceptionForHR(coreApplicationPrivate.CreateNonImmersiveView(out var coreView));

            Win32Window win32Window = Win32Window.FromHwnd(coreWindow.GetHwnd());
            if (!config.IsVisible)
            {
                win32Window.IsCloaked = true;
                win32Window.ShowInTaskBar = false;
            }

            // Mount Xaml rendering
            // Window will be created here (It attaches a subclass to CoreWindow)
            XamlFrameworkView frameworkView = new();
            frameworkView.Initialize(coreView);
            // CoreWindow will be shown in "SetWindow".   
            // ToDo: Prevent call to "ShowWindow"
            frameworkView.SetWindow(coreWindow);

            // Get xaml window
            XamlWindow window = XamlWindow.Current;

            // Attach subclass to customize behavior of "CoreWindow"
            // Our subclass has to be attached after "FrameworkView"!
            XamlWindowSubclass subclass = XamlWindowSubclass.Attach(window);
            subclass.CurrentFrameworkView = frameworkView;

            if (subclass.WindowPrivate != null)
            {
                // A XamlWindow inside a Win32 process is transparent by default
                // (See Windows.UI.Xaml.dll!DirectUI::DXamlCore::ConfigureCoreWindow)
                // This is to provide a consistent behavior across platforms
                subclass.WindowPrivate.TransparentBackground = config.HasTransparentBackground;
            }

            // Enable acrylic "HostBackdropBrush"
            subclass.EnableHostBackdropBrush();

            // Sync settings from "XamlWindowConfig"
            win32Window.HasWin32Frame = config.HasWin32Frame;
            win32Window.IsTopMost = config.IsTopMost;
            subclass.HasWin32TitleBar = config.HasWin32TitleBar;

            // Dispose subclass on close
            coreWindow.Closed += (CoreWindow window, CoreWindowEventArgs args) => subclass.Dispose();

            if (!config.IsVisible)
            {
                subclass.WindowPrivate?.Hide();
                win32Window.IsCloaked = false;
                win32Window.ShowInTaskBar = true;
            }
            else
                window.Activate();// Show window

            return (coreView, window);
        }
    }
}
