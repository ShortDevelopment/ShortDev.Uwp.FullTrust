using System;
using System.Runtime.InteropServices;
using Windows.UI.Core;

namespace Windows.UI.Xaml.Hosting;

[ComImport]
[Guid("a49dea01-9e75-49f0-beee-ef1592fbc82b"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IXamlPresenterStatics3 : IXamlPresenterStatics2
{
    IXamlPresenter2 CreateFromCoreWindow([In] CoreWindow coreWindow);
}
