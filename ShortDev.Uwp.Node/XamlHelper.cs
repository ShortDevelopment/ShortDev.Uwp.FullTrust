using Microsoft.UI.Xaml.Markup;
using ShortDev.Uwp.FullTrust.Xaml;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using WinRT;

namespace ShortDev.Uwp.Node;

/// <summary>
/// Helper to interact with WinRT and Xaml apis
/// </summary>
[JSExport]
public static partial class XamlHelper
{
    static Application? _app;

    static ReflectionXamlMetadataProvider _metadataProvider = null!;
    static IXamlType GetXamlType(string typeName, string? typeNamespace)
    {
        IXamlType? xamlType = null;
        if (!string.IsNullOrEmpty(typeNamespace))
            xamlType = _metadataProvider.GetXamlType($"{typeNamespace}.{typeName}");
        else
            xamlType = _app.As<IXamlMetadataProvider>().GetXamlType($"Microsoft.UI.Xaml.Controls.{typeName}");

        xamlType ??= _metadataProvider.GetXamlType($"Windows.UI.Xaml.Controls.{typeName}");

        if (xamlType is not null)
            return xamlType;

        throw new InvalidOperationException($"Type \"{typeName}\" could not be found");
    }
    static IXamlMember GetXamlMember(object obj, string propertyName)
    {
        var xamlType = _metadataProvider.GetXamlType(obj.GetType()) ?? throw new InvalidOperationException($"Invalid type {obj.GetType()}");
        return xamlType.GetMember(propertyName) ?? throw new ArgumentException($"Invalid member {propertyName}", nameof(propertyName));
    }

    /// <summary>
    /// Initializes the WinUI App singleton
    /// </summary>
    public static async Task InitializeAsync()
    {
        if (_app is not null)
            return;

        //var files = await Task.WhenAll(
        //    Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.pri")
        //        .Select(async path => await StorageFile.GetFileFromPathAsync(path))
        //);
        //ResourceManager.Current.LoadPriFiles(files);

        _metadataProvider = new();
        _app = new WinUIApp();

        var window = XamlWindowFactory.CreateNewWindow(new("Node.js"));
        window.Content = new Frame();
        window.Activate();

        // _app.Resources = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
    }

    /// <summary>
    /// Get the root element
    /// </summary>
    /// <returns></returns>
    public static object GetRootElement()
        => Window.Current.Content;

    /// <summary>
    /// Creates a xaml element
    /// </summary>
    /// <param name="typeName">tag-name</param>
    /// <param name="typeNamespace">Optionally the namespace of the type</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If the type could not be found</exception>
    public static object CreateElement(string typeName, string? typeNamespace)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return GetXamlType(typeName, typeNamespace).ActivateInstance();
    }

    /// <summary>
    /// Get value of a property
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static object GetValue(object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(propertyName);

        return GetXamlMember(obj, propertyName).GetValue(obj);
    }

    /// <summary>
    /// Set value of a property
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    public static void SetValue(object obj, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        ArgumentNullException.ThrowIfNull(value);

        var member = GetXamlMember(obj, propertyName);
        member.SetValue(obj,
            XamlBindingHelper.ConvertValue(member.Type.UnderlyingType, value)
        );
    }

    static readonly ConditionalWeakTable<object, Dictionary<string, RoutedEventHandler>> _handlers = [];

    /// <summary>
    /// Add event handler
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="eventName"></param>
    /// <param name="handler"></param>
    public static void SetEventHandler(object obj, string eventName, Action<object> handler)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(eventName);

        var type = obj.GetType();
        var @event = type.GetEvent(eventName) ?? throw new ArgumentException($"No event {eventName} on type {type.Name}");

        var handlerMap = _handlers.GetOrCreateValue(obj);
        if (handlerMap.TryGetValue(eventName, out var oldHandler))
            @event.RemoveEventHandler(obj, oldHandler);

        RoutedEventHandler newHandler = (sender, args) => handler(sender);
        @event.AddEventHandler(obj, newHandler);
        handlerMap[eventName] = newHandler;
    }

    /// <summary>
    /// Get parent element
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    public static object? GetParent(object child)
    {
        ArgumentNullException.ThrowIfNull(child);

        return VisualTreeHelper.GetParent((DependencyObject)child);
    }

    /// <summary>
    /// Get the next sibling
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static object? GetNextSibling(object child)
    {
        ArgumentNullException.ThrowIfNull(child);

        var childElement = (UIElement)child;
        if (VisualTreeHelper.GetParent(childElement) is not Panel panel)
            throw new InvalidOperationException("Cannot get children from non-panel parent");

        var newIndex = panel.Children.IndexOf(childElement) + 1;
        if (newIndex >= panel.Children.Count)
            return null;

        return panel.Children[newIndex];
    }

    /// <summary>
    /// Add element as child of another element
    /// </summary>
    /// <param name="child"></param>
    /// <param name="parent"></param>
    /// <param name="anchor"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void InsertChild(object child, object parent, object? anchor)
    {
        ArgumentNullException.ThrowIfNull(child);
        ArgumentNullException.ThrowIfNull(parent);

        if (parent is ContentControl contentControl)
        {
            contentControl.Content = child;
            return;
        }

        var childElement = (UIElement)child;
        if (parent is Panel panel)
        {
            if (anchor is not UIElement anchorElement)
            {
                panel.Children.Add(childElement);
                return;
            }

            var insertionIndex = panel.Children.IndexOf(anchorElement);
            panel.Children.Insert(insertionIndex, childElement);
            return;
        }

        throw new InvalidOperationException($"Cannot add Child to {parent.GetType().Name}");
    }

    /// <summary>
    /// Removes an element from the visual tree
    /// </summary>
    /// <param name="child"></param>
    public static void RemoveChild(object child)
    {
        ArgumentNullException.ThrowIfNull(child);

        var childElement = (UIElement)child;
        if (VisualTreeHelper.GetParent(childElement) is not Panel panel)
            throw new InvalidOperationException("Cannot remove child from non-panel parent");

        panel.Children.Remove(childElement);
    }

    /// <summary>
    /// Processes all current events in the message loop
    /// </summary>
    public static void ProcessEvents()
    {
        Window.Current.Dispatcher.ProcessEvents(Windows.UI.Core.CoreProcessEventsOption.ProcessOneIfPresent);
    }
}
