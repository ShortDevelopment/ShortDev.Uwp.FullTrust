using System;
using System.Runtime.InteropServices;

namespace Windows.UI.ViewManagement
{
    [Guid("00924ac0-932b-4a6b-9c4b-dc38c82478ce"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IApplicationViewTitleBar
    {
        Color? BackgroundColor { get; set; }

        Color? ButtonBackgroundColor { get; set; }

        Color? ButtonForegroundColor { get; set; }

        Color? ButtonHoverBackgroundColor { get; set; }

        Color? ButtonHoverForegroundColor { get; set; }

        Color? ButtonInactiveBackgroundColor { get; set; }

        Color? ButtonInactiveForegroundColor { get; set; }

        Color? ButtonPressedBackgroundColor { get; set; }

        Color? ButtonPressedForegroundColor { get; set; }

        Color? ForegroundColor { get; set; }

        Color? InactiveBackgroundColor { get; set; }

        Color? InactiveForegroundColor { get; set; }
    }
}
