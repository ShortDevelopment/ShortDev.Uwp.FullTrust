﻿using ShortDev.Uwp.FullTrust.Types;
using System;
using System.Runtime.InteropServices;

namespace ApplicationFrame
{
    [Guid("143715d9-a015-40ea-b695-d5cc267e36ee")] // 143715d9_a015_40ea_b695_d5cc267e36ee
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IApplicationFrame
    {
        [PreserveSig]
        int GetFrameWindow(out IntPtr hWnd);

        [Obsolete("Wrong signature")]
        void SetPosition();

        [PreserveSig]
        int GetPresentedWindow(out IntPtr hWnd);

        [PreserveSig]
        int SetPresentedWindow(IntPtr hWnd);

        [Obsolete("Wrong signature")]
        int SetSystemVisual(int x);

        [Obsolete("Wrong signature")]
        void GetSystemVisual();

        [PreserveSig] // ToDo: No effect?!
        int SetApplicationId([MarshalAs(UnmanagedType.LPWStr)] string appId);

        [PreserveSig]
        int SetMinimumSize(ref Win32Size size);

        [PreserveSig]
        int SetMaximumSize(ref Win32Size size);

        [PreserveSig]
        int FitToWorkArea();

        [PreserveSig]
        int GetChromeOptions(out int options);

        [PreserveSig]
        int SetChromeOptions(int options1, int options2);

        [Obsolete("Wrong signature")]
        void GetChromeOffsets();

        [PreserveSig]
        int GetTitleBarDrawnByApp(out bool value);

        [PreserveSig]
        int InvokeActionsMenu();

        [PreserveSig]
        int GetTitleBar(out IApplicationFrameTitleBar titleBar);

        /// <summary>
        /// Format <c>0x00bbggrr</c>
        /// </summary>
        [PreserveSig]
        int GetBackgroundColor(out int color);

        /// <summary>
        /// Format <c>0x00bbggrr</c>
        /// </summary>
        [PreserveSig]
        int SetBackgroundColor(int color);

        [PreserveSig]
        int GetSystemVisualFadeTime(out uint time);

        [PreserveSig]
        int SetOperatingMode(int newOperatingMode /* = 1 */);

        [PreserveSig]
        int SetSizeConstraintOverridesPhysical(ref tagSIZE size1, ref tagSIZE size2);

        [PreserveSig]
        int SetSizeConstraintOverridesLogical(ref tagSIZE size1, ref tagSIZE size2);

        [PreserveSig]
        int SetPreferredAspectRatioHint(ref tagSIZE ration);

        [Obsolete("Wrong signature")]
        void SetSystemVisualAnimation();
        [Obsolete("Wrong signature")]
        void GetPropertyValue();

        [PreserveSig]
        int EnsureSizeConstraints();

        [Obsolete("Wrong signature")]
        void OnCommand();
        [Obsolete("Wrong signature")]
        void OnCommand2();
        [Obsolete("Wrong signature")]
        void OnCloseCommand();

        [PreserveSig]
        bool IsEqual(IApplicationFrame frame);

        [PreserveSig]
        int Destroy();

        [PreserveSig]
        int NotifyChromeChange(int flags);

        [PreserveSig]
        int NotifyVisibleButtonsChange();

        [Obsolete("Wrong signature")]
        void GetMinimumSize(out tagSIZE size);
    }


    public enum APPLICATION_FRAME_CHROME_OPTIONS
    {
        ZERO = 0,
        OTHER = 97
    }

    public enum NotifyChromeChangeFlags
    {
        ZERO = 0
    }

    public enum FRAME_OPERATING_MODE
    {
        FullScreen = 0,
        Window = 1
    }

    public struct tagSIZE
    {
        public int cx;
        public int cy;
    }
}
