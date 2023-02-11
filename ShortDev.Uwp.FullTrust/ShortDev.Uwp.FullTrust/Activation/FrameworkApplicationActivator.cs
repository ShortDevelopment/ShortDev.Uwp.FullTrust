using ShortDev.Uwp.FullTrust.Interfaces;

namespace ShortDev.Uwp.FullTrust.Activation;

public static class FrameworkApplicationActivator
{
    public static IFrameworkApplicationStaticsPrivate Activate()
        => InteropHelper.RoGetActivationFactory<IFrameworkApplicationStaticsPrivate>("Windows.UI.Xaml.Application");
}
