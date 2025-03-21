using Microsoft.UI.Xaml.XamlTypeInfo;
using ShortDev.Uwp.FullTrust.Xaml;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace ShortDev.Uwp.Compose;

sealed partial class App : FullTrustApplication, IXamlMetadataProvider
{
    public App()
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

    readonly XamlControlsXamlMetaDataProvider _provider = new();
    public IXamlType GetXamlType(Type type)
        => _provider.GetXamlType(type);

    public IXamlType GetXamlType(string fullName)
        => _provider.GetXamlType(fullName);

    public XmlnsDefinition[] GetXmlnsDefinitions()
        => _provider.GetXmlnsDefinitions();
}
