﻿using System;
using System.Runtime.InteropServices;

namespace Shell
{
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.I4)]
        int QueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }

    [Guid("5f75d642-3fd3-46f6-86d3-a4aee3c0ffee")] // 
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IIAMServiceProvider
    {
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.I4)]
        int PivateQueryService(ref Guid guidService, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }
}