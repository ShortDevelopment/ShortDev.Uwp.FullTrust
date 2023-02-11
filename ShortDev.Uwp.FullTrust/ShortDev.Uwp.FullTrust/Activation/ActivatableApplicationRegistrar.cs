using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Activation;

namespace ShortDev.Uwp.FullTrust.Activation;

public static class ActivatableApplicationRegistrar
{
    static readonly Guid CLSID_ActivatableApplicationRegistrar = new("dea794e0-1c1d-4363-b171-98d0b1703586");

    public static void RegisterActivatableApplication(IActivatableApplication app)
    {
        var registrar = (IActivatableApplicationRegistrar)Activator.CreateInstance(
            Type.GetTypeFromCLSID(CLSID_ActivatableApplicationRegistrar)
        );
        Marshal.ThrowExceptionForHR(
            registrar.RegisterActivatableApplication(Marshal.GetIUnknownForObject(app))
        );
    }

    [Guid("036c57fc-e73d-4f3a-b095-8b813fa2f764")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IActivatableApplicationRegistrar
    {
        [PreserveSig]
        int RegisterActivatableApplication(IntPtr activatableApplication);
    }
}

[ComImport]
[Guid("92696c00-7578-48e1-ac1a-2ca909e2c8cf")]
[InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IActivatableApplication
{
    [PreserveSig]
    int Activate(string a, string b, string c, string d, IActivatedEventArgs args, IntPtr p1, IntPtr p2);
}