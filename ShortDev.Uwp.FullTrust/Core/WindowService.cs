using System;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Core
{
    public static class WindowService
    {
        #region IsWindowServiceSupported
        [DllImport("twinapi.appcore.dll", EntryPoint = "#8")]
        static extern bool CoreIsWindowServiceSupported(IntPtr hWnd, ref Guid serviceId);

        public static bool IsWindowServiceSupported<T>(IntPtr hWnd)
        {
            Guid iid = typeof(T).GUID;
            return CoreIsWindowServiceSupported(hWnd, ref iid);
        }
        #endregion

        #region QueryWindowService
        [DllImport("twinapi.appcore.dll", EntryPoint = "#7")]
        static extern int CoreQueryWindowService(IntPtr hWnd, ref Guid serviceId, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object service);

        public static T QueryWindowService<T>(IntPtr hWnd)
            => QueryWindowService<T>(hWnd, typeof(T).GUID, typeof(T).GUID);

        public static T QueryWindowService<T>(IntPtr hWnd, Guid serviceId, Guid iid)
        {
            Marshal.ThrowExceptionForHR(CoreQueryWindowService(hWnd, ref serviceId, ref iid, out object result));
            return (T)result;
        }
        #endregion

        #region CoreUnregisterAllWindowServices
        [DllImport("twinapi.appcore.dll", EntryPoint = "#10")]
        static extern int CoreUnregisterAllWindowServices(IntPtr hWnd);

        public static void UnregisterAllWindowServices(IntPtr hWnd)
            => Marshal.ThrowExceptionForHR(CoreUnregisterAllWindowServices(hWnd));
        #endregion

        [DllImport("twinapi.appcore.dll", EntryPoint = "#11")]
        static extern unsafe int CoreEnumViewServices(IntPtr hWnd, out int length, out Guid* guidPtr);

        public static unsafe Guid[] GetServices(IntPtr hWnd)
        {
            Marshal.ThrowExceptionForHR(CoreEnumViewServices(hWnd, out var length, out var ptr));
            Guid[] guids = new Guid[length];
            for (int i = 0; i < length; i++)
                guids[i] = ptr[i];
            return guids;
        }
    }
}
