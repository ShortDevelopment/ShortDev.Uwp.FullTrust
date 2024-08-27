using Microsoft.UI.Xaml.XamlTypeInfo;
using System.Runtime.InteropServices;

namespace ShortDev.Uwp.Node;
internal sealed class WinUIApp : Application, IXamlMetadataProvider
{
    public WinUIApp()
    {
        UnhandledException += (s, e) =>
        {
            e.Handled = true;
            Console.WriteLine($"Unhandled Expection: {e.Exception}\nStackTrace: {e.Exception.StackTrace}");
        };
    }

    readonly XamlControlsXamlMetaDataProvider _metadataProvider = new();

    public IXamlType GetXamlType(Type type)
        => _metadataProvider.GetXamlType(type);

    public IXamlType GetXamlType(string fullName)
        => _metadataProvider.GetXamlType(fullName);

    public XmlnsDefinition[] GetXmlnsDefinitions()
        => _metadataProvider.GetXmlnsDefinitions();
}
