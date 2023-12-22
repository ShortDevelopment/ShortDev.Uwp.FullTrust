using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace ShortDev.Uwp.FullTrust.Activation;

internal sealed class Win32LaunchActivationArgs : IActivatedEventArgs, ILaunchActivatedEventArgs, ILaunchActivatedEventArgs2, IApplicationViewActivatedEventArgs, IPrelaunchActivatedEventArgs
{
    public ActivationKind Kind { get; } = ActivationKind.Launch;

    public ApplicationExecutionState PreviousExecutionState { get; } = ApplicationExecutionState.NotRunning;

    public string Arguments { get; } = string.Join(' ', Environment.GetCommandLineArgs());

    public SplashScreen SplashScreen => throw new NotImplementedException();

    public string TileId { get; } = "App";

    public TileActivatedInfo TileActivatedInfo => throw new NotImplementedException();

    public int CurrentlyShownApplicationViewId { get; }
        = ApplicationView.GetApplicationViewIdForWindow(CoreWindow.GetForCurrentThread());

    public bool PrelaunchActivated { get; } = false;
}
