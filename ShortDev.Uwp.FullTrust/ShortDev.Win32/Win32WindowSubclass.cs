using ShortDev.Uwp.FullTrust;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ShortDev.Win32;

public sealed class Win32WindowSubclass
{
    #region Singleton
    static readonly ConcurrentDictionary<IntPtr, Win32WindowSubclass> _subclassRegistry = new();

    /// <summary>
    /// Attaches a <see cref="Win32WindowSubclass"/> to a given <see cref="XamlWindow"/>. <br/>
    /// Only one subclass ist allowed per window!
    /// </summary>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentNullException" />
    public static Win32WindowSubclass Attach(XamlWindow window)
        => Attach(window.GetHwnd());

    internal static Win32WindowSubclass Attach(IntPtr hWnd)
    {
        if (_subclassRegistry.ContainsKey(hWnd))
            throw new ArgumentException($"Window already has a subclass!");

        Win32WindowSubclass subclass = new(hWnd);
        subclass.Install();
        Debug.Assert(_subclassRegistry.TryAdd(hWnd, subclass));

        return subclass;
    }

    /// <summary>
    /// Returns an existing <see cref="Win32WindowSubclass"/> for a given <see cref="XamlWindow"/>
    /// </summary>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="KeyNotFoundException" />
    public static Win32WindowSubclass ForWindow(XamlWindow window)
        => _subclassRegistry[window.GetHwnd()];

    /// <inheritdoc cref="ForWindow(XamlWindow)"/>
    public static Win32WindowSubclass GetForCurrentView()
        => ForWindow(XamlWindow.Current);
    #endregion

    #region Instance
    public IntPtr Hwnd { get; }
    public Win32Window Win32Window { get; private set; }

    private Win32WindowSubclass(IntPtr hwnd)
    {
        Hwnd = hwnd;
        Win32Window = Win32Window.FromHwnd(hwnd);
    }

    void OnDestroy()
    {
        try
        {
            Uninstall();
            _subclassRegistry.TryRemove(Hwnd, out _);
        }
        catch { }
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

    unsafe LRESULT XamlWindowSubclassProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam, nuint id, nuint data)
    {
        foreach (var filter in Filters)
        {
            if (filter.PreFilterMessage(hwnd, (int)msg, wParam, lParam, id, out var result))
                return (LRESULT)result;
        }

        if (ExtendsContentIntoTitleBar && DwmDefWindowProc(hwnd, msg, wParam, lParam, out var dwmResult))
            return dwmResult;

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

        if (msg == WM_NCHITTEST)
        {
            if (IsPointInTitleBar(GetClientCoord(lParam)))
                return (LRESULT)2;
            // return (IntPtr)(-1); // Nowhere
        }

        if (!HasWin32TitleBar && msg == WM_NCCALCSIZE)
        {
            // https//github.com/microsoft/terminal/blob/ff8fdbd2431f1cfd8211833815be481dfdec4420/src/cascadia/WindowsTerminal/NonClientIslandWindow.cpp#L405
            var nccspOld = (NCCALCSIZE_PARAMS*)(IntPtr)lParam;
            var topOld = nccspOld->rgrc._0.top;

            // Run default processing
            var result = DefSubclassProc(hwnd, msg, wParam, lParam);

            var nccsp = (NCCALCSIZE_PARAMS*)(IntPtr)lParam;
            // Rest to old top (remove title bar)
            nccsp->rgrc._0.top = topOld;
            return result;
        }

        // https://docs.microsoft.com/en-us/windows/win32/learnwin32/closing-the-window
        if (msg == WM_CLOSE)
        {
            if (CloseRequested != null)
            {
                const int CANCEL = 0;
                if (_currentCloseRequest == null)
                {
                    _currentCloseRequest = new(this);
                    CloseRequested.Invoke(this, _currentCloseRequest);
                }

                if (_currentCloseRequest.IsDeferred) // User clicked "Close" again
                    return (LRESULT)CANCEL; // Still waiting for user choise

                // Deferral of "XamlWindowCloseRequestedEventArgs" will call "Close" again
                if (_currentCloseRequest.Handled)
                {
                    _currentCloseRequest = null; // Allow for event to be resent
                    return (LRESULT)CANCEL; // User chose to cancel "Close"
                }
            }

            OnDestroy();
        }

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

    #region TitleBar
    UIElement? _titleBarElement;
    public void SetTitleBar(UIElement? value)
        => _titleBarElement = value;

    bool IsPointInTitleBar(Point p)
    {
        if (_titleBarElement?.XamlRoot == null || p.X < 0 || p.Y < 0)
            return false;

        var ele = VisualTreeHelper.FindElementsInHostCoordinates(p, _titleBarElement.XamlRoot.Content, false).FirstOrDefault();
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
    Win32WindowCloseRequestedEventArgs? _currentCloseRequest;

    /// <summary>
    /// Occurs when the user invokes the system button for close (the 'x' button in the corner of the app's title bar).
    /// </summary>
    public event EventHandler<Win32WindowCloseRequestedEventArgs>? CloseRequested;
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
