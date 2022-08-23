using ShortDev.Uwp.FullTrust.Internal;
using System;
using System.Runtime.InteropServices;
using Windows.UI.Composition;

namespace ShortDev.Win32
{
    public sealed class Win32Window
    {
        private Win32Window() { }

        public IntPtr Hwnd { get; private set; }

        public static Win32Window FromHwnd(IntPtr hwnd)
        {
            Win32Window window = new();
            window.Hwnd = hwnd;
            return window;
        }

        internal void NotifyFrameChanged()
        {
            // https://github.com/strobejb/winspy/blob/03887c8ab1ebc9abad6865743eba15b94c9e9dbc/src/StyleEdit.c#L143
            SetWindowPos(
                Hwnd, IntPtr.Zero,
                0, 0, 0, 0,
                SetWindowPosFlags.IgnoreMove | SetWindowPosFlags.IgnoreResize | SetWindowPosFlags.IgnoreZOrder | SetWindowPosFlags.DoNotActivate | SetWindowPosFlags.FrameChanged
            );
        }

        #region SetWindowLong
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public IntPtr SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong, bool notifyWindow = true)
        {
            IntPtr result;
            if (IntPtr.Size == 8)
                result = SetWindowLong64(hWnd, nIndex, new IntPtr(dwNewLong));
            else
                result = SetWindowLong32(hWnd, nIndex, dwNewLong);

            if (notifyWindow)
                NotifyFrameChanged();

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        #endregion

        #region GetWindowLong
        public IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
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
        #endregion

        #region TopMost
        bool _isTopMost = false;
        public bool IsTopMost
        {
            get => _isTopMost;
            set
            {
                // ToDo: This activates the window...
                //if (value == _isTopMost)
                //    return;

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
                int hRes = DesktopWindowManager.SetWindowAttributeInternal(Hwnd, (DwmWindowAttribute)19, ref value, sizeof(int));
                if (hRes != 0)
                    Marshal.ThrowExceptionForHR(DesktopWindowManager.SetWindowAttributeInternal(Hwnd, (DwmWindowAttribute)20, ref value, sizeof(int)));
                NotifyFrameChanged();
                _useDarkMode = value;
            }
        }
        #endregion

        /// <summary>
        /// Enables the HostBackdrop brush for acrylic
        /// </summary>
        public unsafe void EnableHostBackdropBrush()
        {
            // Windows.UI.Xaml.dll!DirectUI::Window::EnableHostBackdropBrush
            WindowCompositionAttribData dwAttribute;
            dwAttribute.Attrib = WindowCompositionAttrib.ACCENT_POLICY;
            AccentPolicy policy;
            policy.AccentState = AccentState.ENABLE_HOSTBACKDROP;
            dwAttribute.pvData = &policy;
            dwAttribute.cbData = (uint)Marshal.SizeOf(policy);
            WindowCompositionHelper.SetWindowCompositionAttribute(Hwnd, ref dwAttribute);
        }

        /// <summary>
        /// Gets or sets wether the window is drawn. <br/>
        /// See <see href="https://docs.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute#:~:text=DWM_CLOAKED_APP,window."/>
        /// </summary>
        public bool IsCloaked
        {
            get
            {
                Marshal.ThrowExceptionForHR(DesktopWindowManager.GetWindowAttributeInternal(
                    Hwnd,
                    DwmWindowAttribute.CLOAKED,
                    out var value,
                    sizeof(int)
                ));
                return value != 0;
            }
            set
            {
                Marshal.ThrowExceptionForHR(DesktopWindowManager.SetWindowAttributeInternal(
                    Hwnd,
                    DwmWindowAttribute.CLOAK,
                    ref value,
                    sizeof(int)
                ));
            }
        }
    }
}
