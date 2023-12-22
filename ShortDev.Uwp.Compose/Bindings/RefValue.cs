using Windows.UI.Xaml;

namespace ShortDev.Uwp.Compose.Bindings;

public sealed class RefValue<T> : DependencyObject
{
    public RefValue(T defaultValue)
        => Value = defaultValue;

    public T Value
    {
        get => (T)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    internal static DependencyProperty ValueProperty { get; } = DependencyProperty.Register(nameof(Value), typeof(T), typeof(RefValue<T>), new(default));
}
