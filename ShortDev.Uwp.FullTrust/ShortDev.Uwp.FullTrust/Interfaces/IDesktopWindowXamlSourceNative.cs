using System;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Interfaces
{
    [Guid("3cbcf1bf-2f76-4e9c-96ab-e84b37972554"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDesktopWindowXamlSourceNative
    {
        [PreserveSig]
        int AttachToWindow(IntPtr parentHwnd);
    }
}
