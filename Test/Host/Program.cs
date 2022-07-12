using ShortDev.Uwp.FullTrust.Activation;
using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Uwp.FullTrust.Types;
using ShortDev.Uwp.FullTrust.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UwpUI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace VBAudioRouter.Host
{
    static class Program
    {
        static IntPtr testWindowHwnd;

        [STAThread]
        static void Main()
        {
            // https://raw.githubusercontent.com/fboldewin/COM-Code-Helper/master/code/interfaces.txt
            // GOOGLE: "IApplicationViewCollection" site:lise.pnfsoftware.com

            FullTrustApplication.Start((param) => new App(), new("Test") { HasTransparentBackground = true });

            using (XamlApplicationWrapper appWrapper = new(() => new App()))
            {
                //XamlWindowActivator.CreateNewThread(() =>
                //{
                //    var window2 = XamlWindowActivator.CreateNewFromXaml(new("XamlTest"), File.OpenRead("TestPage.xaml.txt"));
                //    window2.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
                //});

                var window = XamlWindowActivator.CreateNewWindow(new("Test"));
                window.Content = new MainPage();

                CoreWindow coreWindow = window.CoreWindow;

                var subclass = Windows.UI.Xaml.Window.Current.GetSubclass();
                subclass.CloseRequested += Program_CloseRequested1;
                subclass.UseDarkMode = true;

                var hWnd = (coreWindow as object as ICoreWindowInterop).WindowHandle;
                testWindowHwnd = hWnd;

                //var bandId = WindowBandHelper.ZBandID.ImmersiveNotification;
                //Marshal.ThrowExceptionForHR(WindowBandHelper.SetWindowBand(hWnd, IntPtr.Zero, bandId));
                //Marshal.ThrowExceptionForHR(WindowBandHelper.GetWindowBand((IntPtr)hWnd, out var band));

                #region ApplicationFrame
                var frameManager = ApplicationFrameActivator.CreateApplicationFrameManager();
                var immersiveShell = ImmersiveShellActivator.CreateImmersiveShellServiceProvider();

                var uncloakService = immersiveShell.QueryService<IImmersiveApplicationManager>() as IUncloakWindowService;
                var frameService = immersiveShell.QueryService<IImmersiveApplicationManager>() as IApplicationFrameService;
                var applicationPresentation = immersiveShell.QueryService<IImmersiveApplicationManager>() as IImmersiveApplicationPresentation;
                // ListAllFrames(frameManager);

                //{
                //    var applicationView = ApplicationView.GetForCurrentView(); // ✔
                //    applicationView.IsScreenCaptureEnabled = false; // ✔
                //    applicationView.TitleBar.BackgroundColor = Windows.UI.Colors.Red; // ❌
                //    applicationView.TryEnterFullScreenMode(); // ❌
                //    var visualizationSettings = PointerVisualizationSettings.GetForCurrentView(); // ✔
                //    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                //    SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += Program_CloseRequested;
                //}

                //var provider = immersiveShell.QueryService<IIAMServiceProvider>();
                //frameService = provider.PrivateQueryService<IApplicationFrameService>();

                //Marshal.ThrowExceptionForHR(frameService.GetFrameByWindow((IntPtr)0x80A7A, out var proxy));
                //proxy.Test();

                //var frameFactory = immersiveShell.QueryService<IApplicationFrameFactory>();
                //Marshal.ThrowExceptionForHR(frameFactory.CreateFrameWithWrapper(out var frameWrapper));

                //var frame = CreateNewFrame(frameManager);

                //{ // Show frame
                //    Marshal.ThrowExceptionForHR(frame.GetFrameWindow(out IntPtr frameHwnd));
                //    RemoteThread.UnCloakWindowShell(frameHwnd);
                //}

                //Marshal.ThrowExceptionForHR(frame.SetPresentedWindow(hWnd));

                //var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                //titleBar.BackgroundColor = Windows.UI.Colors.Red
                //var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
                // IApplicationFrameTitleBarPersistenceInternal GUID_1f4df06b_6e3b_46ab_9365_55568e176b53
                #endregion

                coreWindow.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);

                //Marshal.ThrowExceptionForHR(frame.Destroy());
            }
        }

        private static async void Program_CloseRequested1(object sender, ShortDev.Uwp.FullTrust.Xaml.Navigation.XamlWindowCloseRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            MessageDialog dialog = new("Do you want to close the app?");
            dialog.Commands.Clear();
            dialog.Commands.Add(new UICommand("Cancel"));
            dialog.Commands.Add(new UICommand("Close"));
            if ((await dialog.ShowAsync()).Label != "Close")
                e.Handled = true;
            deferral.Complete();
        }

        private static void Program_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static IApplicationFrame CreateNewFrame(IApplicationFrameManager frameManager)
        {
            Marshal.ThrowExceptionForHR(frameManager.CreateFrame(out var frame));

            // SetApplicationId
            Marshal.ThrowExceptionForHR(frame.SetOperatingMode(1));
            // SetMinimumSize
            // SetSystemVisual
            // SetPresentedWindow (0)
            // SetPreferredAspectRatioHint
            // SetSizeConstraintOverridesPhysical
            Marshal.ThrowExceptionForHR(frame.SetChromeOptions(0, 0));
            // SetPosition
            // Marshal.ThrowExceptionForHR(frame.SetOperatingMode(1));
            // Marshal.ThrowExceptionForHR(frame.SetSystemVisual(0));
            Marshal.ThrowExceptionForHR(frame.SetBackgroundColor(65535));

            Marshal.ThrowExceptionForHR(frame.GetTitleBar(out var titleBar));
            Marshal.ThrowExceptionForHR(titleBar.SetWindowTitle($"LK Window - {DateTime.Now}"));
            Marshal.ThrowExceptionForHR(titleBar.SetVisibleButtons(2, 2));
            return frame;
        }

        #region List Frames
        private static void ListAllFrames(IApplicationFrameManager frameManager)
        {
            #region Immersive Shell
            var serviceProvider = ImmersiveShellActivator.CreateImmersiveShellServiceProvider();

            IApplicationViewCollection viewCollection = serviceProvider.QueryService<IApplicationViewCollection>();
            IApplicationViewCollectionManagement viewCollectionManagement = (IApplicationViewCollectionManagement)viewCollection;
            #endregion

            Marshal.ThrowExceptionForHR(frameManager.GetFrameArray(out var frameArray));
            Marshal.ThrowExceptionForHR(frameArray.GetCount(out var count));
            bool alreadyGotVictim = false;
            for (uint i = 0; i < count; i++)
            {
                IApplicationFrame frame = frameArray.GetAt<IApplicationFrame>(i);
                Marshal.ThrowExceptionForHR(frame.GetChromeOptions(out var options));
                Marshal.ThrowExceptionForHR(frame.GetFrameWindow(out var hwndHost));
                frame.GetPresentedWindow(out var hwndContent);

                var view = GetApplicationViewForFrame(viewCollection, frame);
                string appUserModelId = "";
                view?.GetAppUserModelId(out appUserModelId);
                if (view != null && !alreadyGotVictim)
                {
                    Marshal.ThrowExceptionForHR(view.SetCloak(ApplicationViewCloakType.DEFAULT, false));
                    Marshal.ThrowExceptionForHR(view.Flash());
                    // Marshal.ThrowExceptionForHR(view.SetCloak(ApplicationViewCloakType.VIRTUAL_DESKTOP, false));
                    Marshal.ThrowExceptionForHR(frame.SetPresentedWindow(testWindowHwnd));

                    //Marshal.ThrowExceptionForHR(frame.SetBackgroundColor(System.Drawing.Color.Green.ToArgb()));
                    //Marshal.ThrowExceptionForHR(frame.GetTitleBar(out var titleBar));
                    //Marshal.ThrowExceptionForHR(frame.SetApplicationId("Microsoft.WindowsCalculator_8wekyb3d8bbwe!App"));
                    alreadyGotVictim = true;
                }

                Debug.Print(
                    $"HWND: {hwndHost}; TITLE: {GetWindowTitle(hwndHost)};\r\n" +
                    $"CONTENT: {hwndContent}; TITLE: {GetWindowTitle(hwndContent)};\r\n" +
                    $"OPTIONS: {options}\r\n" +
                    $"ID: {appUserModelId}\r\n"
                );
            }
        }

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [Flags]
        public enum test
        {
            a = 0x1,
            b = 0x2,
            c = 0x3
        }

        private static IApplicationView? GetApplicationViewForFrame(IApplicationViewCollection collection, IApplicationFrame frame)
        {
            try
            {
                Marshal.ThrowExceptionForHR(frame.GetFrameWindow(out IntPtr hwnd));
                Marshal.ThrowExceptionForHR(collection.GetViewForHwnd(hwnd, out var view));
                return view;
            }
            catch
            {
                return null;
            }
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder stringBuilder = new();
            Marshal.ThrowExceptionForHR(GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity));
            return stringBuilder.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        #endregion
    }
}
