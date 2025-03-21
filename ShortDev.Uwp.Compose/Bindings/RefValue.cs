using System.ComponentModel;

namespace ShortDev.Uwp.Compose.Bindings;

public sealed partial class RefValue<T> : INotifyPropertyChanged, INotifyPropertyChanging
{
    public RefValue(T defaultValue)
        => Value = defaultValue;

    public T Value
    {
        get => field;
        set
        {
            PropertyChanging?.Invoke(this, PropertyChangingEventArgs);
            field = value;
            PropertyChanged?.Invoke(this, PropertyChangedEventArgs);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangingEventHandler? PropertyChanging;

    static readonly PropertyChangedEventArgs PropertyChangedEventArgs = new(nameof(Value));
    static readonly PropertyChangingEventArgs PropertyChangingEventArgs = new(nameof(Value));
}
