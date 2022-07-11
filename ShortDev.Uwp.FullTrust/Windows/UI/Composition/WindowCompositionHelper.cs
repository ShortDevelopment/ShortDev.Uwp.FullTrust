using System;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Windows.UI.Composition
{
    public static class WindowCompositionHelper
    {
        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WindowCompositionAttribData
        {
            public WindowCompositionAttrib Attrib;
            public AccentPolicy* pvData;
            public uint cbData;
        }

        public enum WindowCompositionAttrib : int
        {
            WCA_UNDEFINED = 0x0,
            WCA_NCRENDERING_ENABLED = 0x1,
            WCA_NCRENDERING_POLICY = 0x2,
            WCA_TRANSITIONS_FORCEDISABLED = 0x3,
            WCA_ALLOW_NCPAINT = 0x4,
            WCA_CAPTION_BUTTON_BOUNDS = 0x5,
            WCA_NONCLIENT_RTL_LAYOUT = 0x6,
            WCA_FORCE_ICONIC_REPRESENTATION = 0x7,
            WCA_EXTENDED_FRAME_BOUNDS = 0x8,
            WCA_HAS_ICONIC_BITMAP = 0x9,
            WCA_THEME_ATTRIBUTES = 0xA,
            WCA_NCRENDERING_EXILED = 0xB,
            WCA_NCADORNMENTINFO = 0xC,
            WCA_EXCLUDED_FROM_LIVEPREVIEW = 0xD,
            WCA_VIDEO_OVERLAY_ACTIVE = 0xE,
            WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 0xF,
            WCA_DISALLOW_PEEK = 0x10,
            WCA_CLOAK = 0x11,
            WCA_CLOAKED = 0x12,
            WCA_ACCENT_POLICY = 0x13,
            WCA_FREEZE_REPRESENTATION = 0x14,
            WCA_EVER_UNCLOAKED = 0x15,
            WCA_VISUAL_OWNER = 0x16,
            WCA_HOLOGRAPHIC = 0x17,
            WCA_EXCLUDED_FROM_DDA = 0x18,
            WCA_PASSIVEUPDATEMODE = 0x19,
            WCA_USEDARKMODECOLORS = 0x1A,
            WCA_LAST = 0x1B,
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
            ACCENT_DISABLED = 0x0,
            ACCENT_ENABLE_GRADIENT = 0x1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 0x2,
            ACCENT_ENABLE_BLURBEHIND = 0x3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 0x4,
            ACCENT_ENABLE_HOSTBACKDROP = 0x5,
            ACCENT_INVALID_STATE = 0x6,
        };
    }
}
