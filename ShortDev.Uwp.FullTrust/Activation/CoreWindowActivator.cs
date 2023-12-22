using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Internal.Windows.UI.Core;
using System.Runtime.InteropServices.Marshalling;

namespace ShortDev.Uwp.FullTrust.Activation;

public static class CoreWindowActivator
{
    public enum CoreWindowType : int
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
        CoreWindowType windowType,
        [MarshalAs(UnmanagedType.BStr)] string windowTitle,
        int x,
        int y,
        uint width,
        uint height,
        uint dwAttributes,
        ref nint hOwnerWindow,
        ref Guid riid,
        out nint pWindow
    );

    public static CoreWindow CreateCoreWindow(CoreWindowType windowType, string windowTitle)
        => CreateCoreWindow(windowType, windowTitle, null);

    public static CoreWindow CreateCoreWindow(CoreWindowType windowType, string windowTitle, Rect? dimensions)
    {
        var rect = dimensions ?? GenerateDefaultWindowPosition();
        return CreateCoreWindow(windowType, windowTitle, rect, nint.Zero);
    }

    public static CoreWindow CreateCoreWindow(CoreWindowType windowType, string windowTitle, Rect dimensions, nint hOwnerWindow, uint dwAttributes = 0)
    {
        Guid iid = typeof(ICoreWindowInterop).GUID;
        Marshal.ThrowExceptionForHR(PrivateCreateCoreWindow(windowType, windowTitle, (int)dimensions.Left, (int)dimensions.Top, (uint)dimensions.Width, (uint)dimensions.Height, dwAttributes, ref hOwnerWindow, ref iid, out var pWindow));
        return CoreWindow.FromAbi(pWindow);
    }

    /// <summary>
    /// <see href="https://devblogs.microsoft.com/oldnewthing/20131122-00/?p=2593"/>
    /// </summary>
    /// <returns></returns>
    public static unsafe Rect GenerateDefaultWindowPosition()
    {
        const int CW_USEDEFAULT = unchecked((int)0x80000000);
        const string CLASS_NAME = "tmp";

        var hInstance = (HINSTANCE)Process.GetCurrentProcess().Handle;

        fixed (char* pClassName = CLASS_NAME)
        {
            WNDCLASSEXW wc = new();
            wc.cbSize = (uint)Marshal.SizeOf(wc);

            wc.lpfnWndProc = (a, b, c, d) => (LRESULT)1;
            wc.hInstance = hInstance;
            wc.lpszClassName = pClassName;

            RegisterClassEx(wc);
        }

        var hwnd = CreateWindowEx(
            0,                                  // Optional window styles.
            CLASS_NAME,                         // Window class
            CLASS_NAME,                         // Window text
            WINDOW_STYLE.WS_OVERLAPPEDWINDOW,   // Window style

            // Size and position
            CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,

            HWND.Null,                          // Parent window    
            HMENU.Null,                         // Menu
            hInstance,                          // Instance handle
            (void*)0                           // Additional application data
        );
        if (hwnd == nint.Zero)
            throw new Win32Exception();

        GetWindowRect(hwnd, out var _bounds);

        DestroyWindow(hwnd);

        return new(_bounds.left, _bounds.top, _bounds.right - _bounds.left, _bounds.bottom - _bounds.top);
    }

    [DllImport("windows.ui.core.textinput.dll", EntryPoint = "#1500")]
    extern static int CreateTextInputProducer(/* ITextInputConsumer */ nint consumer, out /* ITextInputProducer */ nint pProducer);

    public static ITextInputProducer CreateTextInputProducer(CoreWindow coreWindow)
    {
        var consumer = coreWindow.As<ITextInputConsumer>();
        var pConsumer = MarshalInterface<ITextInputConsumer>.FromManaged(consumer);
        Marshal.ThrowExceptionForHR(
            CreateTextInputProducer(pConsumer, out var pProducer)
        );
        var producer = MarshalInterface<ITextInputProducer>.FromAbi(pProducer);
        consumer.TextInputProducer = producer;
        return producer;
    }
}
