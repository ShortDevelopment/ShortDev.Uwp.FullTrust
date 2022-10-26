using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using WinUI.Interop.CoreWindow;
using static Windows.Win32.PInvoke;
using XamlWindow = Windows.UI.Xaml.Window;

namespace ShortDev.Uwp.FullTrust.Xaml
{
    public sealed class XamlWindowSubclass : IDisposable
    {
        #region Singleton
        static Dictionary<IntPtr, XamlWindowSubclass> _subclassRegistry = new();

        /// <summary>
        /// Attaches a <see cref="XamlWindowSubclass"/> to a given <see cref="XamlWindow"/>. <br/>
        /// Only one subclass ist allowed per window!
        /// </summary>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public static XamlWindowSubclass Attach(XamlWindow window)
        {
            XamlWindowSubclass subclass = Attach(window.GetHwnd());
            subclass.Window = window;
            subclass.WindowPrivate = window as object as IWindowPrivate;
            return subclass;
        }

        internal static XamlWindowSubclass Attach(IntPtr hWnd)
        {
            if (_subclassRegistry.ContainsKey(hWnd))
                throw new ArgumentException($"Window already has a subclass!");

            XamlWindowSubclass subclass = new(hWnd);
            subclass.Install();
            _subclassRegistry.Add(hWnd, subclass);

            return subclass;
        }

        /// <summary>
        /// Returns an existing <see cref="XamlWindowSubclass"/> for a given <see cref="XamlWindow"/>
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="KeyNotFoundException" />
        public static XamlWindowSubclass ForWindow(XamlWindow window)
            => _subclassRegistry[window.GetHwnd()];

        /// <inheritdoc cref="ForWindow(XamlWindow)"/>
        public static XamlWindowSubclass GetForCurrentView()
            => ForWindow(XamlWindow.Current);
        #endregion

        #region Instance
        public IntPtr Hwnd { get; }
        public Win32Window Win32Window { get; private set; }

        public XamlWindow? Window { get; private set; }
        public IWindowPrivate? WindowPrivate { get; private set; }
        public FrameworkView? CurrentFrameworkView { get; internal set; }

        private XamlWindowSubclass(IntPtr hwnd)
        {
            Hwnd = hwnd;
            Win32Window = Win32Window.FromHwnd(hwnd);
        }

        bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(XamlWindowSubclass));
            _disposed = true;

            Uninstall();
            _subclassRegistry.Remove(Hwnd);

            GC.KeepAlive(this);
        }
        #endregion

        #region Subclass Installation
        SUBCLASSPROC? _subclassProc;
        IntPtr? _subclassProcPtr;
        void Install()
        {
            if (_subclassProc != null)
                throw new InvalidOperationException();

            _subclassProc = XamlWindowSubclassProc;
            _subclassProcPtr = Marshal.GetFunctionPointerForDelegate(_subclassProc);
            SetWindowSubclass((HWND)Hwnd, _subclassProc, 0, 0);
        }

        void Uninstall()
        {
            if (_subclassProc == null)
                throw new InvalidOperationException();

            RemoveWindowSubclass((HWND)Hwnd, _subclassProc, 0);
            _subclassProc = null;
        }
        #endregion

        LRESULT XamlWindowSubclassProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam, nuint id, nuint data)
        {
            foreach (var filter in Filters)
            {
                if (filter.PreFilterMessage(hwnd, (int)msg, wParam, lParam, id, out var result))
                    return (LRESULT)result;
            }

            if (ExtendsContentIntoTitleBar && DwmDefWindowProc(hwnd, msg, wParam, lParam, out var dwmResult))
                return dwmResult;

            const uint WM_ACTIVATE = 0x0006;
            if (msg == WM_ACTIVATE)
            {
                MARGINS margins = new()
                {
                    cxLeftWidth = 0,
                    cxRightWidth = 0,
                    cyBottomHeight = 0,
                    cyTopHeight = -1
                };
                DwmExtendFrameIntoClientArea(hwnd, margins);
                // SetWindowTheme(hwnd, "", "");
            }

            const uint WM_NCHITTEST = 0x0084;
            if (msg == WM_NCHITTEST)
            {
                if (IsPointInTitleBar(GetClientCoord(lParam)))
                    return (LRESULT)2;
                // return (IntPtr)(-1); // Nowhere
            }

            const int WM_NCCALCSIZE = 0x83;
            if (!HasWin32TitleBar && msg == WM_NCCALCSIZE)
            {
                // https//github.com/microsoft/terminal/blob/ff8fdbd2431f1cfd8211833815be481dfdec4420/src/cascadia/WindowsTerminal/NonClientIslandWindow.cpp#L405
                var topOld = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam).rgrc._0.top;

                // Run default processing
                var result = DefSubclassProc(hwnd, msg, wParam, lParam);

                var nccsp = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);
                // Rest to old top (remove title bar)
                nccsp.rgrc._0.top = topOld;
                Marshal.StructureToPtr(nccsp, lParam, true);
                return result;
            }

            // https://docs.microsoft.com/en-us/windows/win32/learnwin32/closing-the-window
            const int WM_CLOSE = 0x0010;
            if (msg == WM_CLOSE)
            {
                const int CANCEL = 0;
                if (_currentCloseRequest == null)
                {
                    if (CloseRequested != null)
                    {
                        Navigation.XamlWindowCloseRequestedEventArgs args = new(this);
                        CloseRequested?.Invoke(this, args);
                        if (args.IsDeferred || args.Handled)
                            return (LRESULT)CANCEL;
                    }
                }
                else
                {
                    if (_currentCloseRequest.IsDeferred) // User clicked "Close" again
                        return (LRESULT)CANCEL; // Still waiting for user choise
                    else
                    {
                        // Deferral of "XamlWindowCloseRequestedEventArgs" will call "Close" again
                        if (_currentCloseRequest.Handled)
                            return (LRESULT)CANCEL; // User chose to cancel "Close"
                        _currentCloseRequest = null; // Allow for event to be resent
                    }
                }
            }

            const int WM_NCPAINT = 0x85;
            if (msg == WM_NCPAINT)
            {
                //var hdc = PInvoke.GetWindowDC(new HWND(hwnd));
                //using (Graphics g = Graphics.FromHdc(hdc))
                //{
                //    g.Clear(Color.Red);
                //}
                //PInvoke.ReleaseDC(new HWND(hwnd), hdc);
                //return IntPtr.Zero;
            }

            const int WM_PAINT = 0xF;
            if (msg == WM_PAINT) { }

            return DefSubclassProc(hwnd, msg, wParam, lParam);
        }

        public bool ExtendsContentIntoTitleBar { get; set; } = true;

        #region Win32 TitleBar
        bool _hasWin32TitleBar = true;
        /// <summary>
        /// If <see langword="false"/>, the window will have no (win32) titlebar
        /// </summary>
        public bool HasWin32TitleBar
        {
            get => _hasWin32TitleBar;
            set
            {
                _hasWin32TitleBar = value;
                Win32Window.NotifyFrameChanged();
            }
        }
        #endregion

        public void Activate()
        {
            Window?.Activate();
            Win32Window.BringToFront();
        }

        #region TitleBar
        UIElement? _titleBarElement;
        public void SetTitleBar(UIElement? value)
            => _titleBarElement = value;

        bool IsPointInTitleBar(Point p)
        {
            if (_titleBarElement == null || p.X < 0 || p.Y < 0)
                return false;

            var ele = VisualTreeHelper.FindElementsInHostCoordinates(p, null, false).FirstOrDefault();
            return ele == _titleBarElement;
        }

        Point GetClientCoord(LPARAM lParam)
        {
            int x = unchecked((short)lParam);
            int y = unchecked((short)((int)lParam >> 16));
            System.Drawing.Point point = new(x, y);
            ScreenToClient((HWND)Hwnd, ref point);
            double scale = GetDpiForWindow((HWND)Hwnd) / 96.0;
            return new(point.X / scale, point.Y / scale);
        }
        #endregion

        #region CloseRequested
        Navigation.XamlWindowCloseRequestedEventArgs? _currentCloseRequest;

        /// <summary>
        /// Occurs when the user invokes the system button for close (the 'x' button in the corner of the app's title bar).
        /// </summary>
        public event EventHandler<Navigation.XamlWindowCloseRequestedEventArgs>? CloseRequested;
        #endregion

        #region MessageFilter
        public List<IMessageFilter> Filters = new();

        /// <summary>
        /// Filters out a window message.
        /// </summary>
        public interface IMessageFilter
        {
            /// <summary>
            /// Filters out a message. <br/>
            /// <see langword="true" /> to filter the message; <see langword="false" /> to pass the message to the next filter or the <see cref="XamlWindow"/>.
            /// </summary>
            bool PreFilterMessage(IntPtr hwnd, int msg, nuint wParam, nint lParam, nuint id, out IntPtr result);
        }
        #endregion
    }
}
