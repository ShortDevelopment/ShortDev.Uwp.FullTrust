using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.UI.Core;
using Windows.Win32.Foundation;
using Windows.Win32.Security;

namespace ShortDev.Uwp.FullTrust;

internal static class InteropHelper
{
    public static unsafe T RoGetActivationFactory<T>(string activatableClassId)
    {
        Guid iid = typeof(T).GUID;

        HSTRING classId = default;
        WindowsCreateStringReference(activatableClassId, (uint)activatableClassId.Length, out _, &classId).ThrowOnFailure();
        RoGetActivationFactory(classId, iid, out var pFactory).ThrowOnFailure();

        return MarshalInterface<T>.FromAbi(pFactory);
    }

    [DllImport("api-ms-win-core-winrt-l1-1-0.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows8.0")]
    internal static extern unsafe HRESULT RoGetActivationFactory(HSTRING activatableClassId, in Guid iid, out nint pFactory);

    public static HWND GetHwnd(this XamlWindow window)
        => window.CoreWindow.GetHwnd();

    public static HWND GetHwnd(this CoreWindow coreWindow)
        => coreWindow.As<ICoreWindowInterop>().WindowHandle;

    static readonly HANDLE CurrentProcessPseudoToken = (HANDLE)(-4);
    public static unsafe bool IsAppContainer
    {
        get
        {
            uint result = 0;
            uint size = sizeof(uint);
            if (!GetTokenInformation(CurrentProcessPseudoToken, TOKEN_INFORMATION_CLASS.TokenIsAppContainer, &result, size, out size))
                throw new Win32Exception();

            return Convert.ToBoolean(result);
        }
    }

    public static unsafe bool HasPackageIdentity
    {
        get
        {
            uint length = 0;
            GetCurrentPackageFullName(ref length, null);

            char* pPkgFamilyName = stackalloc char[(int)length];

            var err = GetCurrentPackageFullName(ref length, new PWSTR(pPkgFamilyName));
            if (err == WIN32_ERROR.NO_ERROR)
                return true;

            if (err == WIN32_ERROR.APPMODEL_ERROR_NO_PACKAGE)
                return false;

            throw new Win32Exception();
        }
    }
}