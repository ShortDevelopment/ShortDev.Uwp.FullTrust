using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Xaml.Hosting;

[ComImport]
[Guid("d0c1e6c3-1d35-4770-9c3b-e3ff2eefcc25"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IXamlPresenterStatics2 : IXamlPresenterStatics
{
    IXamlPresenter Current { get; }
}
