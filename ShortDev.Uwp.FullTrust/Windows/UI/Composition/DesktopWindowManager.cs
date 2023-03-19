using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Composition;

public class DesktopWindowManager
{
    public static void SetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, bool value)
        => Marshal.ThrowExceptionForHR(SetWindowAttributeInternal(hwnd, attr, ref value, Marshal.SizeOf<bool>()));

    public static void SetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, int value)
        => Marshal.ThrowExceptionForHR(SetWindowAttributeInternal(hwnd, attr, ref value, Marshal.SizeOf<bool>()));

    [DllImport("dwmapi.dll", PreserveSig = true, EntryPoint = "DwmSetWindowAttribute")]
    internal static extern int SetWindowAttributeInternal(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

    [DllImport("dwmapi.dll", PreserveSig = true, EntryPoint = "DwmSetWindowAttribute")]
    internal static extern int SetWindowAttributeInternal(IntPtr hwnd, DwmWindowAttribute attr, ref bool attrValue, int attrSize);


    [DllImport("dwmapi.dll", PreserveSig = true, EntryPoint = "DwmGetWindowAttribute")]
    internal static extern int GetWindowAttributeInternal(IntPtr hwnd, DwmWindowAttribute attr, out int attrValue, int attrSize);
}

public enum DwmWindowAttribute
{
    NCRENDERING_ENABLED = 1,
    NCRENDERING_POLICY,
    TRANSITIONS_FORCEDISABLED,
    ALLOW_NCPAINT,
    CAPTION_BUTTON_BOUNDS,
    NONCLIENT_RTL_LAYOUT,
    FORCE_ICONIC_REPRESENTATION,
    FLIP3D_POLICY,
    EXTENDED_FRAME_BOUNDS,
    HAS_ICONIC_BITMAP,
    DISALLOW_PEEK,
    EXCLUDED_FROM_PEEK,
    CLOAK,
    CLOAKED,
    FREEZE_REPRESENTATION,
    PASSIVE_UPDATE_MODE,
    USE_HOSTBACKDROPBRUSH,
    USE_IMMERSIVE_DARK_MODE = 20,
    WINDOW_CORNER_PREFERENCE = 33,
    BORDER_COLOR,
    CAPTION_COLOR,
    TEXT_COLOR,
    VISIBLE_FRAME_BORDER_THICKNESS,
    SYSTEMBACKDROP_TYPE,
    LAST
}
