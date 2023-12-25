using ShortDev.Uwp.Compose;
using ShortDev.Uwp.FullTrust.Core;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static ShortDev.Uwp.Compose.Utils;

Run(() =>
{
    TextBlock tb = null!;
    var counter = Ref<long>(0);

    return Compose<NavigationView>(new() { },
        Compose<StackPanel>(new() { Orientation = Orientation.Vertical, Spacing = 5, Background = Resource<Brush>("ApplicationPageBackgroundThemeBrush") }, [
            Compose<TextBlock>(ref tb)
                .Bind(TextBlock.TextProperty, counter, converterFn: x => $"Some Text: {x} clicks"),

            Compose<TextBlock>()
            .Bind(TextBlock.TextProperty, counter),

            Compose<Button>(new() { Content = "Click Me" })
            .OnClick(async (s, e) =>
            {
                counter.Value++;
                tb.Foreground = Brush(Color.FromArgb(255, 255, 100, 100));

                FolderPicker picker = new();
                picker.InitializeWithCoreWindow();
                await picker.PickSingleFolderAsync();
            })
        ])
    );
});