using System;
using System.Runtime.InteropServices;
using Windows.UI.Core;

namespace ShortDev.Uwp.FullTrust.Activation;

public static unsafe class CoreWindowFactoryActivator
{
    public static CoreWindowFactory* CreateInstance()
    {
        const string CLSID_CoreUICoreWindowFactoryProxy = "B243A9FD-C57A-4D3E-A7CF-21CAED64CB5A";
        Guid clsid = new(CLSID_CoreUICoreWindowFactoryProxy);
        Guid iid = typeof(ICoreWindowFactory).GUID;
        Marshal.ThrowExceptionForHR(InteropHelper.CoCreateInstance(ref clsid, 0, 1026, ref iid, out var pFactory));
        return (CoreWindowFactory*)pFactory;
    }

    public static unsafe CoreWindowFactory* CreateInstance(CoreWindowActivator.CoreWindowType windowType)
    {
        var factory = CreateInstance();
        var ptr = (uint*)factory + 31;
        *ptr = (uint)windowType;
        return factory;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CoreWindowFactory
    {
        public CoreWindowFactory_Vtbl* vtbl;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CoreWindowFactory_Vtbl
    {
        void* QueryInterface;
        void* AddRef;
        void* Release;
        void* GetIids;
        void* GetRuntimeClassName;
        void* GeTrustLevel;
        public delegate* unmanaged[Stdcall]<CoreWindowFactory*, string, void**, int> CreateCoreWindow;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate int CreateCoreWindowSig(CoreWindowFactory* @this, [MarshalAs(UnmanagedType.HString)] string windowTitle, out CoreWindow window);
}
