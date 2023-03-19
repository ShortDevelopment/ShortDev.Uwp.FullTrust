using System;
using System.Runtime.InteropServices;
using Shell;

namespace ShortDev.Uwp.FullTrust.Activation;

public static class ImmersiveShellActivator
{
    public static Shell.IServiceProvider CreateImmersiveShellServiceProvider()
        => InteropHelper.ComCreateInstance<Shell.IServiceProvider>("c2f03a33-21f5-47fa-b4bb-156362a2f239")!;

    public static T QueryService<T>(this Shell.IServiceProvider serviceProvider)
    {
        Guid iid = typeof(T).GUID;
        Marshal.ThrowExceptionForHR(serviceProvider.QueryService(ref iid, ref iid, out object ptr));
        return (T)ptr;
    }

    public static T PrivateQueryService<T>(this IIAMServiceProvider serviceProvider)
    {
        Guid iid = typeof(T).GUID;
        Marshal.ThrowExceptionForHR(serviceProvider.PivateQueryService(ref iid, ref iid, out object ptr));
        return (T)ptr;
    }
}
