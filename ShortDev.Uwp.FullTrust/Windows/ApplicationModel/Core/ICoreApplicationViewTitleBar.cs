using System.Runtime.InteropServices;
using Windows.Foundation;

namespace Windows.ApplicationModel.Core
{
    [Guid("006d35e3-e1f1-431b-9508-29b96926ac53"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface ICoreApplicationViewTitleBar
    {
        bool ExtendViewIntoTitleBar { get; set; }

        double Height { get; }

        bool IsVisible { get; }

        double SystemOverlayLeftInset { get; }

        double SystemOverlayRightInset { get; }

        event TypedEventHandler<ICoreApplicationViewTitleBar, object> IsVisibleChanged;

        event TypedEventHandler<ICoreApplicationViewTitleBar, object> LayoutMetricsChanged;
    }
}
