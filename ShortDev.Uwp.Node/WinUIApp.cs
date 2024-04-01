using ShortDev.Uwp.FullTrust.Xaml;

namespace ShortDev.Uwp.Node;
internal sealed class WinUIApp : FullTrustApplication
{
    public WinUIApp()
    {
        UnhandledException += ComposeApp_UnhandledException;
    }

    private void ComposeApp_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        throw e.Exception;
    }

    protected override IReadOnlyList<IXamlMetadataProvider> MetadataProviders { get; } = [];
}
