using System;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Interfaces
{
    [ComImport, Guid("45d64a29-a63e-4cb6-b498-5781d298cb4f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICoreWindowInterop
    {
        IntPtr WindowHandle { get; }
        bool MessageHandled { get; }
    }
}
