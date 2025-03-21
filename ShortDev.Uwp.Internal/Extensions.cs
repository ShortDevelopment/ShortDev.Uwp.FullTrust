using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using WinRT;

namespace ShortDev.Uwp.Internal;

public static class Extensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OnActivated")]
    static extern void OnActivated(Application app, IActivatedEventArgs args);

    public static void OnAppActivated(this Application @this, IActivatedEventArgs args)
    {
        OnActivated(@this, args);

        return;

        var app = (IApplicationOverrides)@this;
        app.OnActivated(args);

        // https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/get-activation-info-for-packaged-apps#supported-activation-types
        switch (args.Kind)
        {
            case ActivationKind.ShareTarget:
                app.OnShareTargetActivated(args.As<ShareTargetActivatedEventArgs>());
                break;
            case ActivationKind.File:
                app.OnFileActivated(args.As<FileActivatedEventArgs>());
                break;
            case ActivationKind.Launch:
                app.OnLaunched(args.As<LaunchActivatedEventArgs>());
                break;
        }
    }
}
