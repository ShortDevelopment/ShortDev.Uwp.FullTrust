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

        [DllImport("Kernel32.dll")]
        static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, RemoteThread.MemoryProtection flNewProtect, out RemoteThread.MemoryProtection lpflOldProtect);
        delegate int ICoreWindow_ActivateProc();

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
            // CoreWindow will be activated in "SetWindow".   
            // Proxy prevents activation
            IntPtr ppWindow = Marshal.GetComInterfaceForObject(coreWindow, typeof(ICoreWindow));
            unsafe
            {
                const int ICoreWindow_vtbl_Length = 58;
                var ppv = *(IntPtr**)ppWindow;
                ICoreWindow_ActivateProc activateShim = () =>
                {
                    return 0;
                };
                const int CoreWindow_ActivateOffset = 18;
                VirtualProtect((IntPtr)ppv + CoreWindow_ActivateOffset, (uint)IntPtr.Size, RemoteThread.MemoryProtection.ReadWrite, out _);
                ppv[CoreWindow_ActivateOffset] = Marshal.GetFunctionPointerForDelegate(activateShim);
            }
            ((_IFrameworkView)(object)frameworkView).SetWindow(ppWindow);

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

        [Guid("faab5cd0-8924-45ac-ad0f-a08fae5d0324"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        interface _IFrameworkView
        {
            void Initialize([In] CoreApplicationView applicationView);
            void SetWindow([In] IntPtr window);
            void Load([In] string entryPoint);
            void Run();
            void Uninitialize();
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
