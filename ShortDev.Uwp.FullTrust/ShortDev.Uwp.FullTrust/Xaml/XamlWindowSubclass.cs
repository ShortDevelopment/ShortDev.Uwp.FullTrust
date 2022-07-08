using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Uwp.FullTrust.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI.Xaml;
using XamlWindow = Windows.UI.Xaml.Window;

namespace ShortDev.Uwp.FullTrust.Xaml
{
    public sealed class XamlWindowSubclass : IDisposable
    {
        #region Singleton
        static Dictionary<XamlWindow, XamlWindowSubclass> _subclassRegistry = new();

        /// <summary>
        /// Attaches a <see cref="XamlWindowSubclass"/> to a given <see cref="XamlWindow"/>. <br/>
        /// Only one subclass ist allowed per window!
        /// </summary>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        public static XamlWindowSubclass Attach(XamlWindow window)
        {
            if (_subclassRegistry.ContainsKey(window))
                throw new ArgumentException($"{nameof(window)} already has a subclass!");

            XamlWindowSubclass subclass = new(window);
            subclass.Install();
            _subclassRegistry.Add(window, subclass);

            return subclass;
        }

        /// <summary>
        /// Returns an existing <see cref="XamlWindowSubclass"/> for a given <see cref="XamlWindow"/>
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="KeyNotFoundException" />
        public static XamlWindowSubclass ForWindow(XamlWindow window)
            => _subclassRegistry[window];

        /// <inheritdoc cref="ForWindow(XamlWindow)"/>
        public static XamlWindowSubclass GetForCurrentView()
            => ForWindow(XamlWindow.Current);
        #endregion

        #region Instance
        public FrameworkView? CurrentFrameworkView { get; internal set; }

        public XamlWindow Window { get; }
        public IWindowPrivate? WindowPrivate { get; }

        public IntPtr Hwnd
            => Window.GetHwnd();

        private XamlWindowSubclass(XamlWindow window)
        {
            this.Window = window;
            this.WindowPrivate = window as object as IWindowPrivate;
            Debug.Assert(WindowPrivate != null, $"\"{nameof(WindowPrivate)}\" is null");
        }

        bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(XamlWindowSubclass));
            _disposed = true;

            Uninstall();
            _subclassRegistry.Remove(this.Window);

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
                if (!CloseAllowed && CloseRequested != null)
                {
                    Navigation.XamlWindowCloseRequestedEventArgs args = new(this);
                    CloseRequested?.Invoke(this, args);
                    if (args.IsDeferred || args.Handled)
                        return (IntPtr)0; // Cancel
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
                NotifyFrameChanged(Hwnd);
            }
        }
        #endregion

        #region Win32 Frame
        bool _minimizeBox = true;
        public bool MinimizeBox
        {
            get => _minimizeBox;
            set
            {
                if (value == _minimizeBox)
                    return;

                _minimizeBox = value;
                UpdateFrameFlags();
            }
        }

        bool _maximizeBox = true;
        public bool MaximizeBox
        {
            get => _maximizeBox;
            set
            {
                if (value == _maximizeBox)
                    return;

                _maximizeBox = value;
                UpdateFrameFlags();
            }
        }

        bool _hasWin32Frame = false;
        public bool HasWin32Frame
        {
            get => _hasWin32Frame;
            set
            {
                if (value == _hasWin32Frame)
                    return;

                _hasWin32Frame = value;
                UpdateFrameFlags();
            }
        }

        void UpdateFrameFlags()
        {
            var flags = (long)GetWindowLong(Hwnd, GWL_STYLE);
            if (HasWin32Frame)
            {
                flags |= (int)WindowStyles.WS_THICKFRAME;
                flags |= (int)WindowStyles.WS_SYSMENU;
                flags |= (int)WindowStyles.WS_DLGFRAME;
                flags |= (int)WindowStyles.WS_BORDER;

                if (MinimizeBox)
                    flags |= (int)WindowStyles.WS_MINIMIZEBOX;
                if (MaximizeBox)
                    flags |= (int)WindowStyles.WS_MAXIMIZEBOX;
            }
            else
            {
                flags &= ~(int)WindowStyles.WS_THICKFRAME;
                flags &= ~(int)WindowStyles.WS_SYSMENU;
                flags &= ~(int)WindowStyles.WS_DLGFRAME;
                flags &= ~(int)WindowStyles.WS_BORDER;

                flags &= ~(int)WindowStyles.WS_MINIMIZEBOX;
                flags &= ~(int)WindowStyles.WS_MAXIMIZEBOX;
            }
            SetWindowLong(Hwnd, GWL_STYLE, flags, notifyWindow: true);
        }

        static void NotifyFrameChanged(IntPtr hWnd)
        {
            // https://github.com/strobejb/winspy/blob/03887c8ab1ebc9abad6865743eba15b94c9e9dbc/src/StyleEdit.c#L143
            SetWindowPos(
                hWnd, IntPtr.Zero,
                0, 0, 0, 0,
                SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.IgnoreZOrder | SetWindowPosFlags.DoNotActivate | SetWindowPosFlags.FrameChanged
            );
        }
        #endregion

        #region TopMost
        bool _isTopMost = false;
        public bool IsTopMost
        {
            get => _isTopMost;
            set
            {
                if (value == _isTopMost)
                    return;

                const int HWND_TOPMOST = -1;
                const int HWND_NOTOPMOST = -2;
                SetWindowPos(Hwnd,
                    value ? (IntPtr)HWND_TOPMOST : (IntPtr)HWND_NOTOPMOST,
                    0, 0, 0, 0,
                    SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize
                );
                _isTopMost = value;
            }
        }
        #endregion

        #region ShowInTaskBar
        bool _showInTaskBar = true;
        public bool ShowInTaskBar
        {
            get => _showInTaskBar;
            set
            {
                if (value == _showInTaskBar)
                    return;

                var flags = (long)GetWindowLong(Hwnd, GWL_EXSTYLE);
                if (!value)
                    flags |= (int)WindowStyles.WS_EX_TOOLWINDOW;
                else
                    flags &= ~(int)WindowStyles.WS_EX_TOOLWINDOW;
                SetWindowLong(Hwnd, GWL_EXSTYLE, flags, notifyWindow: true);
                _showInTaskBar = value;
            }
        }
        #endregion

        #region Dark Mode
        bool _useDarkMode = false;
        public bool UseDarkMode
        {
            get => _useDarkMode;
            set
            {
                // https://github.com/qt/qtbase/blob/1808df9ce59a8c1d426f0361e25120a7852a6442/src/plugins/platforms/windows/qwindowswindow.cpp#L3168
                const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
                int hRes = DwmSetWindowAttribute(Hwnd, 19, ref value, Marshal.SizeOf<bool>());
                if (hRes != 0)
                    Marshal.ThrowExceptionForHR(DwmSetWindowAttribute(Hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(bool)));
                NotifyFrameChanged(Hwnd);
                _useDarkMode = value;
            }
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

        [DllImport("dwmapi.dll", PreserveSig = true, SetLastError = true)]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, TestStruct attr, IntPtr a, IntPtr b);

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct TestStruct
        {
            public int attr;
            public int* v8;
            public int v9;
        }
        #endregion

        #region EnableHostBackdropBrush
        bool _enableHostBackdropBrush = false;
        public bool EnableHostBackdropBrush
        {
            get => _enableHostBackdropBrush;
            set
            {
                // Windows.UI.Xaml.dll!DirectUI::Window::EnableHostBackdropBrush
                const int DWMWA_USE_HOSTBACKDROPBRUSH = 16;
                unsafe
                {
                    int dwvalue = 16;
                    DwmSetWindowAttribute(Hwnd, 19, ref dwvalue, 5);
                    // throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                _useDarkMode = value;
            }
        }
        #endregion

        #region CloseRequested
        internal bool CloseAllowed { get; set; } = false;

        /// <summary>
        /// Occurs when the user invokes the system button for close (the 'x' button in the corner of the app's title bar).
        /// </summary>
        public event EventHandler<Navigation.XamlWindowCloseRequestedEventArgs> CloseRequested;
        #endregion

        #region API

        #region SetWindowLong
        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;
        static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong, bool notifyWindow = true)
        {
            IntPtr result;
            if (IntPtr.Size == 8)
                result = SetWindowLong64(hWnd, nIndex, new IntPtr(dwNewLong));
            else
                result = SetWindowLong32(hWnd, nIndex, dwNewLong);

            if (notifyWindow)
                NotifyFrameChanged(hWnd);

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLong64(hWnd, nIndex);
            else
                return GetWindowLong32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);
        #endregion

        #region SetWindowPos
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [Flags]
        private enum SetWindowPosFlags : uint
        {
            AsynchronousWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            DoNotActivate = 0x0010,
            DoNotCopyBits = 0x0100,
            IgnoreMove = 0x0002,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotRedraw = 0x0008,
            DoNotReposition = 0x0200,
            DoNotSendChangingEvent = 0x0400,
            IgnoreResize = 0x0001,
            IgnoreZOrder = 0x0004,
            ShowWindow = 0x0040,
        }
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
