using System.Runtime.InteropServices;

namespace Windows.UI.Xaml.Hosting;

[ComImport]
[Guid("8438b07a-9ce8-4e22-ab5d-811d84699566"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IXamlPresenter
{
    UIElement Content { get; [param: In] set; }
    void SetAtlasSizeHint([In] uint width, [In] uint height);
    void InitializePresenter();
}
