using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace ShortDev.Uwp.FullTrust.Core.Activation
{
    public static class TitleBarActivator
    {
        static Guid IID_IApplicationViewTitleBar = new Guid("00924ac0-932b-4a6b-9c4b-dc38c82478ce");

        [DllImport("twinapi.appcore.dll", EntryPoint = "#501")]
        static extern int CreateCoreApplicationViewTitleBar(
            ITitleBarClientAdapter clientAdapter,
            IntPtr hWnd,
            out CoreApplicationViewTitleBar titleBar
        );

        [DllImport("twinapi.appcore.dll", EntryPoint = "#502")]
        static extern int CreateApplicationViewTitleBar(
            ITitleBarClientAdapter clientAdapter,
            IntPtr hWnd,
            out ApplicationViewTitleBar titleBar
        );

        [DllImport("CoreUIComponents.dll")]
        static extern int CreateNavigationClientWindowAdapter(IntPtr hWnd, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object ptr);

        public static void CreateNavigationClientWindowAdapter(IntPtr hWnd)
        {
            //Guid iid = new Guid("abf53c57-ee50-5342-b52a-26e3b8cc024f"); // IAsyncOperation<IInspectable *>
            //Marshal.ThrowExceptionForHR(CreateNavigationClientWindowAdapter(hWnd, ref iid, out var result));

            Marshal.ThrowExceptionForHR(CreateCoreApplicationViewTitleBar(new CTitleBarClientAdapter(), hWnd, out var result));
            result.ExtendViewIntoTitleBar = true;
        }

        [ComVisible(true)]
        public class CTitleBarClientAdapter : ITitleBarClientAdapter
        {
            public Color BackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonBackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonHoverBackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonHoverForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonInactiveBackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonInactiveForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonPressedBackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ButtonPressedForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool ExtendsContentIntoTitleBar { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color ForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public double Height { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color InactiveBackgroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Color InactiveForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IntPtr InputRoutingHwnd => throw new NotImplementedException();
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("32bf6a67-dcd6-4417-9e0e-5048f239979e")]
        public interface ITitleBarClientAdapter
        {
            Color BackgroundColor { get; set; }
            Color ButtonBackgroundColor { get; set; }
            Color ButtonForegroundColor { get; set; }
            Color ButtonHoverBackgroundColor { get; set; }
            Color ButtonHoverForegroundColor { get; set; }
            Color ButtonInactiveBackgroundColor { get; set; }
            Color ButtonInactiveForegroundColor { get; set; }
            Color ButtonPressedBackgroundColor { get; set; }
            Color ButtonPressedForegroundColor { get; set; }
            bool ExtendsContentIntoTitleBar { get; set; }
            Color ForegroundColor { get; set; }
            double Height { get; set; }
            Color InactiveBackgroundColor { get; set; }
            Color InactiveForegroundColor { get; set; }
            IntPtr InputRoutingHwnd { get; }
        }
    }
}
