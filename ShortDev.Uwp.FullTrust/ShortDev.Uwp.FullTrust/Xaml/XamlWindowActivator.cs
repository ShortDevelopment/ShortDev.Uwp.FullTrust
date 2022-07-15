using ShortDev.Uwp.FullTrust.Activation;
using ShortDev.Uwp.FullTrust.Interfaces;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
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
#pragma warning disable CS0612 // Type or member is obsolete
            if (XamlApplicationWrapper.Current == null)
                throw new InvalidOperationException($"No instance of \"{nameof(XamlApplicationWrapper)}\" was found!");
#pragma warning restore CS0612 // Type or member is obsolete

            CoreWindow coreWindow = CoreWindowActivator.CreateCoreWindow(CoreWindowActivator.WindowType.NOT_IMMERSIVE, config.Title);

            // Enable async / await
            SynchronizationContext.SetSynchronizationContext(new XamlSynchronizationContext(coreWindow));

            // Create CoreApplicationView
            var coreApplicationPrivate = InteropHelper.RoGetActivationFactory<ICoreApplicationPrivate2>("Windows.ApplicationModel.Core.CoreApplication");
            Marshal.ThrowExceptionForHR(coreApplicationPrivate.CreateNonImmersiveView(out var coreView));

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
            subclass.HasWin32Frame = config.HasWin32Frame;
            subclass.IsTopMost = config.IsTopMost;
            subclass.HasWin32TitleBar = config.HasWin32TitleBar;

            // Dispose subclass on close
            coreWindow.Closed += (CoreWindow window, CoreWindowEventArgs args) =>
            {
                subclass.Dispose();
            };

            // Show window
            if (config.IsVisible)
                window.Activate();

            return (coreView, window);
        }

        /// <summary>
        /// Creates a new <see cref="XamlWindow"/>, loads xaml from a <see cref="Stream"/> and sets it as <see cref="XamlWindow.Content"/>. <br/>
        /// The <see cref="Stream"/> will be disposed automatically!
        /// </summary>
        [Obsolete]
        public static XamlWindow CreateNewFromXaml(XamlWindowConfig config, Stream xamlStream)
        {
            using (xamlStream)
            using (StreamReader reader = new(xamlStream))
                return CreateNewFromXaml(config, reader.ReadToEnd());
        }

        /// <summary>
        /// Creates a new <see cref="XamlWindow"/> and sets xaml as <see cref="XamlWindow.Content"/>. <br/>
        /// </summary>
        [Obsolete]
        public static XamlWindow CreateNewFromXaml(XamlWindowConfig config, string xaml)
        {
            var window = CreateNewWindow(config);
            UIElement content = (UIElement)XamlReader.Load(xaml);
            window.Content = content;
            return window;
        }
    }
}
