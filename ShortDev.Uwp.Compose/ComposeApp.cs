using Microsoft.UI.Xaml.Markup;
using ShortDev.Uwp.FullTrust.Xaml;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace ShortDev.Uwp.Compose;

internal sealed class ComposeApp : FullTrustApplication
{
    public ComposeApp()
    {
        UnhandledException += ComposeApp_UnhandledException;
    }

    private void ComposeApp_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        var ex = e.Exception;
        var msg = e.Message;
    }

    public required Func<UIElement> ContentFactory { get; set; }

    protected override void OnActivated(IActivatedEventArgs args)
    {
        // Resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
        Window.Current.Content = ContentFactory();
        Window.Current.Activate();
    }

    protected override IReadOnlyList<IXamlMetadataProvider> MetadataProviders { get; } = [
        // new Microsoft.UI.Xaml.XamlTypeInfo.XamlControlsXamlMetaDataProvider(),
        new ReflectionXamlMetadataProvider()
    ];
}
