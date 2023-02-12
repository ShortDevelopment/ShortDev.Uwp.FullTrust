using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Xaml.Hosting;

[ComImport]
[Guid("5c6ef05e-f60d-4433-8bc6-40586456afeb"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IXamlPresenterStatics
{
    IXamlPresenter2 CreateFromHwnd([In] IntPtr hwnd);
}
