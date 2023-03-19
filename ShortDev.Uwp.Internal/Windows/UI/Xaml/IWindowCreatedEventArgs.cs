using System.Runtime.InteropServices;

namespace Windows.UI.Xaml
{
    [Guid("31b71470-feff-4654-af48-9b398ab5772b"), InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    internal interface IWindowCreatedEventArgs
    {
        Window Window { get; }
    }
}
