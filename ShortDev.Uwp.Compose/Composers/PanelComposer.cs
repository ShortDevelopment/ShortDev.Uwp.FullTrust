using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ShortDev.Uwp.Compose.Composers;

public readonly ref struct PanelComposer<T>(T value) where T : Panel
{
    public T Value { get; } = value;

    public PanelComposer<T> this[UIElement child]
    {
        get
        {
            Value.Children.Clear();
            Value.Children.Add(child);

            return this;
        }
    }

    public PanelComposer<T> this[IReadOnlyList<UIElement> children]
    {
        get
        {
            Value.Children.Clear();
            foreach (var child in children)
                Value.Children.Add(child);

            return this;
        }
    }

    public static implicit operator T(PanelComposer<T> composer)
        => composer.Value;
}
