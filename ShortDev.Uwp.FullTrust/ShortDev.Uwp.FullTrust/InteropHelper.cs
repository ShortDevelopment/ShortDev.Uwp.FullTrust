using System;
using Windows.Win32;
using Windows.Win32.System.WinRT;

namespace ShortDev.Uwp.FullTrust;

internal static class InteropHelper
{
    public static unsafe T RoGetActivationFactory<T>(string activatableClassId)
    {
        Guid iid = typeof(T).GUID;

        HSTRING classId = default;
        WindowsCreateStringReference(activatableClassId, (uint)activatableClassId.Length, out _, &classId).ThrowOnFailure();
        PInvoke.RoGetActivationFactory(classId, iid, out var ptr).ThrowOnFailure();

        return (T)ptr;
    }
}