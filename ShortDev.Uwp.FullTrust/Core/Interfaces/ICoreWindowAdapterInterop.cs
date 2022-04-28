using System;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Core.Interfaces
{
    [Guid("7a5b6fd1-cd73-4b6c-9cf4-2e869eaf470a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface ICoreWindowAdapterInterop
    {
        object ApplicationViewClientAdapter { get; }
        object CoreApplicationViewClientAdapter { get; }
    }
}
