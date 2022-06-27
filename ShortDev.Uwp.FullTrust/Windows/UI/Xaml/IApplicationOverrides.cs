using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Activation;

namespace Windows.UI.Xaml
{
    [Guid("25f99ff7-9347-459a-9fac-b2d0e11c1a0f"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    internal interface IApplicationOverrides
    {
        void OnActivated([In] IActivatedEventArgs args);
        void OnLaunched([In] ILaunchActivatedEventArgs args);
        void OnFileActivated([In] IFileActivatedEventArgs args);
        void OnSearchActivated([In] ISearchActivatedEventArgs args);
        void OnShareTargetActivated([In] IShareTargetActivatedEventArgs args);
        void OnFileOpenPickerActivated([In] IFileOpenPickerActivatedEventArgs args);
        void OnFileSavePickerActivated([In] IFileSavePickerActivatedEventArgs args);
        void OnCachedFileUpdaterActivated([In] ICachedFileUpdaterActivatedEventArgs args);
        void OnWindowCreated([In] IWindowCreatedEventArgs args);
    }
}
