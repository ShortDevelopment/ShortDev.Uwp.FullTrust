using Windows.Storage.Pickers;
using Windows.UI.Core;
using WinRT.Interop;

namespace ShortDev.Uwp.FullTrust.Core;

public static class CoreWindowExtensions
{
    static nint Hwnd => CoreWindow.GetForCurrentThread().GetHwnd();

    public static void InitializeWithCoreWindow(this FileOpenPicker picker)
        => InitializeWithWindow.Initialize(picker, Hwnd);

    public static void InitializeWithCoreWindow(this FileSavePicker picker)
        => InitializeWithWindow.Initialize(picker, Hwnd);

    public static void InitializeWithCoreWindow(this FolderPicker picker)
        => InitializeWithWindow.Initialize(picker, Hwnd);

    public static void InitializeWithCoreWindow(this IInitializeWithCoreWindow dialog)
        => dialog.Initialize(CoreWindow.GetForCurrentThread());
}
