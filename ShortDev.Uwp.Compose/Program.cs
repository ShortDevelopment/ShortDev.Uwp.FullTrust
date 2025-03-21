using ShortDev.Uwp.Compose;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static ShortDev.Uwp.Compose.Utils;

Run(() =>
{
    TextBlock tb = null!;
    var counter = Ref(1L);

    return new StackPanel()
    {
        Orientation = Orientation.Vertical,
        Spacing = 5,
        Background = Resource<Brush>("ApplicationPageBackgroundThemeBrush"),
        Children =
        {
            new TextBlock()
                .Ref(ref tb)
                .Bind(TextBlock.TextProperty, counter, static x => $"Some Text: {x} clicks"),

            new Button()
            {
                Content = "Click me"
            }
                .OnClick(async (s, e) =>
                {
                    counter.Value++;
                    tb.Foreground = Brush(Color.FromArgb(255, 255, 100, 100));

                    //FolderPicker picker = new();
                    //picker.InitializeWithCoreWindow();
                    //await picker.PickSingleFolderAsync();

                    ContentDialog dialog = new() {
                        Content = new TextBox(),
                        XamlRoot = tb.XamlRoot
                    };
                    await dialog.ShowAsync();
                }),

            new TextBox()
        }
    };
});
