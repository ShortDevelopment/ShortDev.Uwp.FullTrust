using ShortDev.Uwp.Compose.Bindings;
using ShortDev.Uwp.FullTrust.Xaml;
using System.Diagnostics.CodeAnalysis;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ShortDev.Uwp.Compose;

public static class Utils
{
    #region Instance
    public static T Compose<T>(T instance) where T : UIElement
        => instance;

    public static T Compose<T>(T instance, [AllowNull] ref T @ref) where T : UIElement
    {
        @ref = instance;
        return Compose(instance);
    }
    #endregion

    #region No Instance
    public static T Compose<T>() where T : UIElement, new()
        => Compose<T>(new());

    public static T Compose<T>([AllowNull] ref T @ref) where T : UIElement, new()
    {
        T instance = Compose<T>();
        @ref = instance;
        return instance;
    }
    #endregion

    #region Panel
    public static T Compose<T>(T instance, IReadOnlyList<UIElement> children) where T : Panel
    {
        foreach (var child in children)
            instance.Children.Add(child);

        return Compose(instance);
    }

    public static T Compose<T>(T instance, [AllowNull] ref T @ref, IReadOnlyList<UIElement> children) where T : Panel
    {
        @ref = instance;
        return Compose(instance, children);
    }
    #endregion

    #region ContentControl
    public static T Compose<T>(T instance, UIElement child) where T : ContentControl
    {
        instance.Content = child;
        return Compose(instance);
    }

    public static T Compose<T>(T instance, [AllowNull] ref T @ref, UIElement child) where T : ContentControl
    {
        @ref = instance;
        return Compose(instance, child);
    }
    #endregion

    public static RefValue<T> Ref<T>(T defaultValue)
        => new(defaultValue);

    public static T Resource<T>(string key)
        => (T)Application.Current.Resources[key];

    public static SolidColorBrush Brush(Color color)
        => new(color);

    public static Thickness Thickness(double uniformLength)
        => ThicknessHelper.FromUniformLength(uniformLength);

    public static void Run(Func<UIElement> contentFactory)
        => FullTrustApplication.Start(p => _ = new ComposeApp() { ContentFactory = contentFactory });
}
