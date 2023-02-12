using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.UI.Composition;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace ShortDev.Win32;

public sealed class Win32Window
{
    private Win32Window() { }

    public IntPtr Hwnd { get; private set; }

    public static Win32Window FromHwnd(IntPtr hwnd)
        => new()
        {
            Hwnd = hwnd
        };

    internal void NotifyFrameChanged()
    {
        // https://github.com/strobejb/winspy/blob/03887c8ab1ebc9abad6865743eba15b94c9e9dbc/src/StyleEdit.c#L143
        SetWindowPos(
            (HWND)Hwnd, HWND.Null,
            0, 0, 0, 0,
            SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
        );
    }

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
        var flags = GetWindowLong((HWND)Hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
        if (HasWin32Frame)
        {
            flags |= (int)WINDOW_STYLE.WS_THICKFRAME;
            flags |= (int)WINDOW_STYLE.WS_SYSMENU;
            flags |= (int)WINDOW_STYLE.WS_DLGFRAME;
            flags |= (int)WINDOW_STYLE.WS_BORDER;

            if (MinimizeBox)
                flags |= (int)WINDOW_STYLE.WS_MINIMIZEBOX;
            if (MaximizeBox)
                flags |= (int)WINDOW_STYLE.WS_MAXIMIZEBOX;
        }
        else
        {
            flags &= ~(int)WINDOW_STYLE.WS_THICKFRAME;
            flags &= ~(int)WINDOW_STYLE.WS_SYSMENU;
            flags &= ~(int)WINDOW_STYLE.WS_DLGFRAME;
            flags &= ~(int)WINDOW_STYLE.WS_BORDER;

            flags &= ~(int)WINDOW_STYLE.WS_MINIMIZEBOX;
            flags &= ~(int)WINDOW_STYLE.WS_MAXIMIZEBOX;
        }
        SetWindowLong((HWND)Hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, flags);
        NotifyFrameChanged();
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
            SetWindowPos((HWND)Hwnd,
                value ? (HWND)(IntPtr)HWND_TOPMOST : (HWND)(IntPtr)HWND_NOTOPMOST,
                0, 0, 0, 0,
                SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
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

            var flags = GetWindowLong((HWND)Hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            if (!value)
                flags |= (int)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
            else
                flags &= ~(int)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
            SetWindowLong((HWND)Hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, flags);
            NotifyFrameChanged();
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

    public void BringToFront()
    {
        if (!SetForegroundWindow((HWND)Hwnd))
            throw new Win32Exception();
    }

    void ShowWindowInternal(SHOW_WINDOW_CMD cmd)
    {
        if (!ShowWindow((HWND)Hwnd, cmd))
            throw new Win32Exception();
    }

    public void Show()
        => ShowWindowInternal(SHOW_WINDOW_CMD.SW_SHOW);

    public void Hide()
        => ShowWindowInternal(SHOW_WINDOW_CMD.SW_HIDE);
}
