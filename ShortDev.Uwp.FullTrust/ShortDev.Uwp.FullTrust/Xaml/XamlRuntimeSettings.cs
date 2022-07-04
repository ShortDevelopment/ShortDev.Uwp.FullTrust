using ShortDev.Uwp.FullTrust.Interfaces;

namespace ShortDev.Uwp.FullTrust.Xaml
{
    public sealed class XamlRuntimeSettings
    {
        public static IXamlRuntimeStatics RuntimeSettings
            => InteropHelper.RoGetActivationFactory<IXamlRuntimeStatics>("Windows.UI.Xaml.Hosting.XamlRuntime");
    }
}
