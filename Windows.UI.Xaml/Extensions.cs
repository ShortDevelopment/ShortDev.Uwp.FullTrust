using Windows.ApplicationModel.Activation;
using WinRT;

namespace Windows.UI.Xaml;

public static class Extensions
{
    public static void OnAppActivated(this Application @this, IActivatedEventArgs args, bool validArgs = true)
    {
        var app = @this.As<IApplicationOverrides>();
        app.OnActivated(args);

        if (!validArgs)
            return;

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
