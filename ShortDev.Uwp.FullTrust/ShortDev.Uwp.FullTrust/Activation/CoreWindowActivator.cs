using ShortDev.Uwp.FullTrust.Interfaces;
using ShortDev.Uwp.FullTrust.Types;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Core;

namespace ShortDev.Uwp.FullTrust.Activation
{
    public static class CoreWindowActivator
    {
        public enum WindowType : int
        {
            IMMERSIVE_BODY = 0,
            IMMERSIVE_DOCK,
            IMMERSIVE_HOSTED,
            IMMERSIVE_TEST,
            IMMERSIVE_BODY_ACTIVE,
            IMMERSIVE_DOCK_ACTIVE,
            NOT_IMMERSIVE
        }



        [DllImport("windows.ui.dll", EntryPoint = "#1500")]
        static extern int PrivateCreateCoreWindow(
            WindowType windowType,
            [MarshalAs(UnmanagedType.BStr)] string windowTitle,
            int x,
            int y,
            uint width,
            uint height,
            uint dwAttributes,
            ref IntPtr hOwnerWindow,
            ref Guid riid,
            out ICoreWindowInterop windowRef
        );

        public static CoreWindow CreateCoreWindow(WindowType windowType, string windowTitle)
        {
            var rect = GenerateDefaultWindowPosition();
            return CreateCoreWindow(windowType, windowTitle, rect, IntPtr.Zero);
        }

        public static CoreWindow CreateCoreWindow(WindowType windowType, string windowTitle, Rect dimensions, IntPtr hOwnerWindow, uint dwAttributes = 0)
        {
            Guid iid = typeof(ICoreWindowInterop).GUID;
            Marshal.ThrowExceptionForHR(PrivateCreateCoreWindow(windowType, windowTitle, (int)dimensions.Left, (int)dimensions.Top, (uint)dimensions.Width, (uint)dimensions.Height, dwAttributes, ref hOwnerWindow, ref iid, out ICoreWindowInterop windowRef));
            return (windowRef as object as CoreWindow)!;
        }

        /// <summary>
        /// <see href="https://devblogs.microsoft.com/oldnewthing/20131122-00/?p=2593"/>
        /// </summary>
        /// <returns></returns>
        public static Rect GenerateDefaultWindowPosition()
        {
            const int CW_USEDEFAULT = (unchecked((int)0x80000000));
            const string CLASS_NAME = "tmp";

            Wndproc wndproc = (a, b, c, d) => (IntPtr)1;

            IntPtr hInstance = Process.GetCurrentProcess().Handle;

            WNDCLASSEX wc = new();
            wc.cbSize = Marshal.SizeOf(wc);

            wc.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndproc);
            wc.hInstance = hInstance;
            wc.lpszClassName = CLASS_NAME;

            var atom = RegisterClassEx(ref wc);

            IntPtr hwnd = CreateWindowEx(
                0,                              // Optional window styles.
                CLASS_NAME,    // Window class
                CLASS_NAME,                     // Window text
                WindowStyles.WS_OVERLAPPEDWINDOW,      // Window style

                // Size and position
                CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,

                IntPtr.Zero,        // Parent window    
                IntPtr.Zero,        // Menu
                hInstance,          // Instance handle
                IntPtr.Zero         // Additional application data
            );
            if (hwnd == IntPtr.Zero)
                throw new Win32Exception();

            GetWindowRect(hwnd, out var _bounds);

            DestroyWindow(hwnd);

            return new(_bounds.Left, _bounds.Top, _bounds.Right - _bounds.Left, _bounds.Bottom - _bounds.Top);
        }

        static class WindowStyles
        {
            public const uint WS_OVERLAPPED = 0x00000000;
            public const uint WS_POPUP = 0x80000000;
            public const uint WS_CHILD = 0x40000000;
            public const uint WS_MINIMIZE = 0x20000000;
            public const uint WS_VISIBLE = 0x10000000;
            public const uint WS_DISABLED = 0x08000000;
            public const uint WS_CLIPSIBLINGS = 0x04000000;
            public const uint WS_CLIPCHILDREN = 0x02000000;
            public const uint WS_MAXIMIZE = 0x01000000;
            public const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
            public const uint WS_BORDER = 0x00800000;
            public const uint WS_DLGFRAME = 0x00400000;
            public const uint WS_VSCROLL = 0x00200000;
            public const uint WS_HSCROLL = 0x00100000;
            public const uint WS_SYSMENU = 0x00080000;
            public const uint WS_THICKFRAME = 0x00040000;
            public const uint WS_GROUP = 0x00020000;
            public const uint WS_TABSTOP = 0x00010000;

            public const uint WS_MINIMIZEBOX = 0x00020000;
            public const uint WS_MAXIMIZEBOX = 0x00010000;

            public const uint WS_TILED = WS_OVERLAPPED;
            public const uint WS_ICONIC = WS_MINIMIZE;
            public const uint WS_SIZEBOX = WS_THICKFRAME;
            public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

            // Common Window Styles

            public const uint WS_OVERLAPPEDWINDOW =
                (WS_OVERLAPPED |
                  WS_CAPTION |
                  WS_SYSMENU |
                  WS_THICKFRAME |
                  WS_MINIMIZEBOX |
                  WS_MAXIMIZEBOX);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateWindowEx(
           uint dwExStyle,
           string lpClassName,
           string lpWindowName,
           uint dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam
        );

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool DestroyWindow(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U2)]
        static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out Win32Rect rect);

        delegate IntPtr Wndproc(IntPtr a, IntPtr b, IntPtr c, IntPtr d);
    }
}
