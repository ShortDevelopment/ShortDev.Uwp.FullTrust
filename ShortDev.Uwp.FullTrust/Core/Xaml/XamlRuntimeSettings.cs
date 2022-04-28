using ShortDev.Uwp.FullTrust.Core.Interfaces;

namespace ShortDev.Uwp.FullTrust.Core.Xaml
{
    public sealed class XamlRuntimeSettings
    {
        public static IXamlRuntimeStatics RuntimeSettings
            => InteropHelper.RoGetActivationFactory<IXamlRuntimeStatics>("Windows.UI.Xaml.Hosting.XamlRuntime");
    }
}
