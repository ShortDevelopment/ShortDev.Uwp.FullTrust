using System.Runtime.InteropServices;

namespace Windows.UI.Xaml;

[Guid("b3ab45d8-6a4e-4e76-a00d-32d4643a9f1a"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IFrameworkApplicationPrivate
{
    [PreserveSig]
    int StartOnCurrentThread(ApplicationInitializationCallback callback);
}
