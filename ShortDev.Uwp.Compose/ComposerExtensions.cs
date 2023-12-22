using ShortDev.Uwp.Compose.Bindings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace ShortDev.Uwp.Compose;

public static class ComposerExtensions
{
    public static T Apply<T>(this T @this, Action<T> action) where T : UIElement
    {
        action(@this);
        return @this;
    }

    public static T Bind<T, TBindingValue>(this T @this, DependencyProperty target, RefValue<TBindingValue> refValue, BindingMode mode = BindingMode.OneWay, Func<TBindingValue, object>? converterFn = null, IValueConverter? converter = null) where T : UIElement
    {
        if (converterFn != null)
            converter = new SimpleLambdaConverter<TBindingValue>(converterFn);

        Binding binding = new()
        {
            Source = refValue,
            Path = new("Value"),
            Mode = mode,
            Converter = converter
        };
        BindingOperations.SetBinding(@this, target, binding);

        return @this;
    }

    private sealed class SimpleLambdaConverter<TBindingValue>(Func<TBindingValue, object> converter) : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not TBindingValue refValue)
                return value;

            return converter(refValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    public static T OnClick<T>(this T @this, RoutedEventHandler handler) where T : ButtonBase
    {
        @this.Click += handler;
        return @this;
    }
}
