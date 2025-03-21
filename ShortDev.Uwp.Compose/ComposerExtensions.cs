using ShortDev.Uwp.Compose.Bindings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace ShortDev.Uwp.Compose;

public static partial class ComposerExtensions
{
    public static T Apply<T>(this T @this, Action<T> action) where T : UIElement
    {
        action(@this);
        return @this;
    }

    public static T Bind<T, TValue>(this T @this, DependencyProperty target, RefValue<TValue> refValue, IValueConverter converter) where T : UIElement
        => @this.Bind(target, refValue, x => converter.Convert(x, typeof(TValue), null, null));

    public static T Bind<T, TValue>(this T @this, DependencyProperty target, RefValue<TValue> refValue, Func<TValue, object> converter) where T : UIElement
    {
        SetValue(null, null);
        refValue.PropertyChanged += SetValue;
        return @this;

        void SetValue(object? sender, EventArgs? e)
        {
            if (converter is null)
                @this.SetValue(target, refValue.Value);
            else
                @this.SetValue(target, converter(refValue.Value));
        }
    }

    private sealed partial class LambdaValueConverter<TBindingValue>(Func<TBindingValue, object> converter) : IValueConverter
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

    public static T Ref<T>(this T @this, ref T reference)
    {
        reference = @this;
        return @this;
    }

    public static T Ref<T>(this T @this, RefValue<T> reference)
    {
        reference.Value = @this;
        return @this;
    }

    public static T OnClick<T>(this T @this, RoutedEventHandler handler) where T : ButtonBase
    {
        @this.Click += handler;
        return @this;
    }
}
