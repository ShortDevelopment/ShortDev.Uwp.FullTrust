using System;
using System.Runtime.InteropServices;

namespace Windows.UI.WindowManagement;

[ComImport, Guid("b74ea3bc-43c1-521f-9c75-e5c15054d78c")]
[InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IApplicationWindow_HwndInterop
{
    IntPtr WindowHandle { get; }
}
