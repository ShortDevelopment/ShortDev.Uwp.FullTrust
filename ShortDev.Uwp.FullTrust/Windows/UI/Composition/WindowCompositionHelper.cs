using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Composition;

public static class WindowCompositionHelper
{
    [DllImport("user32.dll")]
    public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct WindowCompositionAttribData
{
    public WindowCompositionAttrib Attrib;
    public void* pvData;
    public uint cbData;
}

public enum WindowCompositionAttrib : int
{
    UNDEFINED = 0x0,
    NCRENDERING_ENABLED = 0x1,
    NCRENDERING_POLICY = 0x2,
    TRANSITIONS_FORCEDISABLED = 0x3,
    ALLOW_NCPAINT = 0x4,
    CAPTION_BUTTON_BOUNDS = 0x5,
    NONCLIENT_RTL_LAYOUT = 0x6,
    FORCE_ICONIC_REPRESENTATION = 0x7,
    EXTENDED_FRAME_BOUNDS = 0x8,
    HAS_ICONIC_BITMAP = 0x9,
    THEME_ATTRIBUTES = 0xA,
    NCRENDERING_EXILED = 0xB,
    NCADORNMENTINFO = 0xC,
    EXCLUDED_FROM_LIVEPREVIEW = 0xD,
    VIDEO_OVERLAY_ACTIVE = 0xE,
    FORCE_ACTIVEWINDOW_APPEARANCE = 0xF,
    DISALLOW_PEEK = 0x10,
    CLOAK = 0x11,
    CLOAKED = 0x12,
    ACCENT_POLICY = 0x13,
    FREEZE_REPRESENTATION = 0x14,
    EVER_UNCLOAKED = 0x15,
    VISUAL_OWNER = 0x16,
    HOLOGRAPHIC = 0x17,
    EXCLUDED_FROM_DDA = 0x18,
    PASSIVEUPDATEMODE = 0x19,
    USEDARKMODECOLORS = 0x1A,
    LAST = 0x1B,
}

[StructLayout(LayoutKind.Sequential)]
public struct AccentPolicy
{
    public AccentState AccentState;
    public uint AccentFlags;
    public uint GradientColor;
    public int AnimationId;
}

public enum AccentState : int
{
    DISABLED = 0x0,
    ENABLE_GRADIENT = 0x1,
    ENABLE_TRANSPARENTGRADIENT = 0x2,
    ENABLE_BLURBEHIND = 0x3,
    ENABLE_ACRYLICBLURBEHIND = 0x4,
    ENABLE_HOSTBACKDROP = 0x5,
    INVALID_STATE = 0x6,
};
