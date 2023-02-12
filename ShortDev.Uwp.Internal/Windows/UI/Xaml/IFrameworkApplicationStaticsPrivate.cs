using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Xaml;

[Guid("c45f3f8c-61e6-4f9a-be88-fe4fe6e64f5f"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IFrameworkApplicationStaticsPrivate
{
    [PreserveSig]
    int StartInCoreWindowHostingMode(WindowCreationParameters initialWindowConfiguration, ApplicationInitializationCallback callback);
}
