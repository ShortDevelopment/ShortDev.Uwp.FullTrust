﻿using System;
using System.Runtime.InteropServices;

namespace Windows.UI.Xaml.Hosting;

/// <summary>
/// Windows.UI.Xaml.Hosting.XamlRuntime
/// </summary>
[Guid("C805B0C0-6210-4E4F-B76A-E894E8B1A4AD"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
public interface IXamlRuntimeStatics
{
    bool EnableImmersiveColors { get; set; }
    bool EnableWebView { get; set; }
}
