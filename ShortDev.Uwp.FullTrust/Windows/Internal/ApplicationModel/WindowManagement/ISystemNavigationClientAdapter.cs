using System;
using System.Runtime.InteropServices;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;

namespace Windows.Internal.ApplicationModel.WindowManagement
{
    /// <summary>
    /// CoreUIComponent.dll <br/>
    /// <see cref="SystemNavigationManager"/>, <see cref="SystemNavigationManagerPreview"/>
    /// </summary>
    [Guid("279c50aa-416e-4d54-babe-b1e657c583e9"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISystemNavigationClientAdapter
    {
        AppViewBackButtonVisibility AppViewBackButtonVisibility { get; set; }
        Color TitleBarButtonPressedTextColorOverride { set; }
        bool CanHandleCloseRequested { set; }

        [Obsolete]
        void OnNavigateBackRequest(IntPtr arg);

        event EventHandler<BackRequestedEventArgs> BackRequested;

        event EventHandler<SystemNavigationCloseRequestedPreviewEventArgs> CloseRequested;

        [Obsolete]
        event EventHandler<BackRequestedEventArgs> FirstChanceBackRequested;
    }
}
