using System.Runtime.InteropServices;
using Windows.Foundation;

namespace Windows.UI.Xaml.Hosting;

[ComImport]
[Guid("1114f710-6d30-4572-b24e-c81cf25f0fa5"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IXamlPresenter2 : IXamlPresenter
{
    ResourceDictionary Resources { get; [param: In] set; }
    Rect Bounds { get; }
    ApplicationTheme RequestedTheme { get; [param: In] set; }
    bool TransparentBackground { get; [param: In] set; }
    void InitializePresenterWithTheme([In] ApplicationTheme requestedTheme);
    void SetCaretWidth([In] int width);
}
