﻿using ShortDev.Win32.Windowing;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.FullTrust.Xaml;

public static class XamlWindowExtensions
{
    #region Animation
    private const int animationDurationMs = 1000;
    public static void ShowAsFlyout(this XamlWindow window)
    {
        nint hwnd = window.GetHwnd();
        if (AnimateWindow(hwnd, animationDurationMs, AnimateWindowFlags.ACTIVATE | AnimateWindowFlags.SLIDE | AnimateWindowFlags.HOR_POSITIVE) != 0)
            throw new Win32Exception();
    }

    public static void HideAsFlyout(this XamlWindow window)
    {
        nint hwnd = window.GetHwnd();
        if (AnimateWindow(hwnd, animationDurationMs, AnimateWindowFlags.HIDE | AnimateWindowFlags.SLIDE | AnimateWindowFlags.HOR_POSITIVE) != 0)
            throw new Win32Exception();
    }

    [DllImport("user32", SetLastError = true), PreserveSig]
    static extern int AnimateWindow(nint hwnd, int time, AnimateWindowFlags flags);

    [Flags]
    enum AnimateWindowFlags
    {
        HOR_POSITIVE = 0x00000001,
        HOR_NEGATIVE = 0x00000002,
        VER_POSITIVE = 0x00000004,
        VER_NEGATIVE = 0x00000008,
        CENTER = 0x00000010,
        HIDE = 0x00010000,
        ACTIVATE = 0x00020000,
        SLIDE = 0x00040000,
        BLEND = 0x00080000
    }
    #endregion

    public static WindowSubclass GetSubclass(this XamlWindow window)
        => WindowSubclass.Attach(window.GetHwnd(), throwIfExists: false);
}
