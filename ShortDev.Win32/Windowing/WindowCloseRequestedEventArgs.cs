using Windows.Foundation;
using Windows.Win32.Foundation;

namespace ShortDev.Win32.Windowing;

public sealed class WindowCloseRequestedEventArgs
{
    readonly WindowSubclass _subclass;
    public WindowCloseRequestedEventArgs(WindowSubclass subclass)
        => _subclass = subclass;

    internal bool IsDeferred { get; private set; } = false;

    /// <summary>
    /// A <see cref="Deferral"/> object for the CloseRequested event.
    /// </summary>
    public Deferral GetDeferral()
    {
        IsDeferred = true;
        return new(() =>
        {
            IsDeferred = false;
            PostMessage((HWND)_subclass.Hwnd, WM_CLOSE, 0, 0);
        });
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the close request is handled by the app. <br />
    /// <see langword="true"/> if the app has handled the close request; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
    /// </summary>
    public bool Handled { get; set; } = false;
}
