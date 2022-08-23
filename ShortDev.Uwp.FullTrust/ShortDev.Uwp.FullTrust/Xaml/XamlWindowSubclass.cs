using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.UI.Xaml;
using WinUI.Interop.CoreWindow;
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
        SubclassProc? _subclassProc;
        IntPtr? _subclassProcPtr;
        void Install()
        {
            if (_subclassProc != null)
                throw new InvalidOperationException();

            _subclassProc = XamlWindowSubclassProc;
            _subclassProcPtr = Marshal.GetFunctionPointerForDelegate(_subclassProc);
            SetWindowSubclass(Hwnd, _subclassProc, IntPtr.Zero, IntPtr.Zero);
        }

        void Uninstall()
        {
            if (_subclassProc == null)
                throw new InvalidOperationException();

            RemoveWindowSubclass(Hwnd, _subclassProc, IntPtr.Zero);
            _subclassProc = null;
        }

        #region WinApi
        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetWindowSubclass(IntPtr hwnd, SubclassProc callback, IntPtr id, IntPtr data);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool RemoveWindowSubclass(IntPtr hwnd, SubclassProc callback, IntPtr id);

        private delegate IntPtr SubclassProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data);

        [DllImport("comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr DefSubclassProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        #endregion
        #endregion

        IntPtr XamlWindowSubclassProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr id, IntPtr data)
        {
            foreach (var filter in Filters)
            {
                if (filter.PreFilterMessage(hwnd, msg, wParam, lParam, id, out var result))
                    return result;
            }

            const uint WM_NCHITTEST = 0x0084;
            if (msg == WM_NCHITTEST)
            {
                if (CursorIsInTitleBar)
                    return (IntPtr)2;
                // return (IntPtr)(-1); // Nowhere
            }

            const int WM_NCCALCSIZE = 0x83;
            if (!HasWin32TitleBar && msg == WM_NCCALCSIZE)
            {
                // https//github.com/microsoft/terminal/blob/ff8fdbd2431f1cfd8211833815be481dfdec4420/src/cascadia/WindowsTerminal/NonClientIslandWindow.cpp#L405
                var topOld = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam).rgrc0.top;

                // Run default processing
                var result = DefSubclassProc(hwnd, msg, wParam, lParam);

                var nccsp = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(lParam);
                // Rest to old top (remove title bar)
                nccsp.rgrc0.top = topOld;
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
                            return (IntPtr)CANCEL;
                    }
                }
                else
                {
                    if (_currentCloseRequest.IsDeferred) // User clicked "Close" again
                        return (IntPtr)CANCEL; // Still waiting for user choise
                    else
                    {
                        // Deferral of "XamlWindowCloseRequestedEventArgs" will call "Close" again
                        if (_currentCloseRequest.Handled)
                            return (IntPtr)CANCEL; // User chose to cancel "Close"
                        _currentCloseRequest = null; // Allow for event to be resent
                    }
                }
            }

            return DefSubclassProc(hwnd, msg, wParam, lParam);
        }

        /// <summary>
        /// If <see langword="true" />, the window will be dragable as if the mouse would be on the titlebar
        /// </summary>
        public bool CursorIsInTitleBar { get; set; } = false;

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

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public bool MinimizeBox
        {
            get => Win32Window.MinimizeBox;
            set => Win32Window.MinimizeBox = value;
        }

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public bool MaximizeBox
        {
            get => Win32Window.MaximizeBox;
            set => Win32Window.MaximizeBox = value;
        }

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public bool HasWin32Frame
        {
            get => Win32Window.HasWin32Frame;
            set => Win32Window.HasWin32Frame = value;
        }

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public bool IsTopMost
        {
            get => Win32Window.IsTopMost;
            set => Win32Window.IsTopMost = value;
        }

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public bool ShowInTaskBar
        {
            get => Win32Window.ShowInTaskBar;
            set => Win32Window.ShowInTaskBar = value;
        }

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public bool UseDarkMode
        {
            get => Win32Window.UseDarkMode;
            set => Win32Window.UseDarkMode = value;
        }

        [Obsolete($"Use \"{nameof(Win32Window)}\" instead")]
        public unsafe void EnableHostBackdropBrush()
            => Win32Window.EnableHostBackdropBrush();

        #region CloseRequested
        Navigation.XamlWindowCloseRequestedEventArgs? _currentCloseRequest;

        /// <summary>
        /// Occurs when the user invokes the system button for close (the 'x' button in the corner of the app's title bar).
        /// </summary>
        public event EventHandler<Navigation.XamlWindowCloseRequestedEventArgs>? CloseRequested;
        #endregion

        #region NCCALCSIZE_PARAMS
        [StructLayout(LayoutKind.Sequential)]
        struct NCCALCSIZE_PARAMS
        {
            public RECT rgrc0, rgrc1, rgrc2;
            public WINDOWPOS lppos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndinsertafter;
            public int x, y, cx, cy;
            public int flags;
        }
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
            bool PreFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, IntPtr id, out IntPtr result);
        }
        #endregion
    }
}
