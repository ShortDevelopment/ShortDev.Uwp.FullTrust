using ShortDev.Uwp.FullTrust.Xaml;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpUI
{
    public sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame frame = new Frame();
            Window.Current.Content = frame;
            frame.Navigate(typeof(MainPage));
            // Window.Current.Activate();

            var subclass = Window.Current.GetSubclass();
            subclass.CloseRequested += Subclass_CloseRequested;
        }

        private async void Subclass_CloseRequested(object sender, ShortDev.Uwp.FullTrust.Xaml.Navigation.XamlWindowCloseRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            MessageDialog dialog = new MessageDialog("Do you want to close the app?");
            dialog.Commands.Clear();
            dialog.Commands.Add(new UICommand("Cancel"));
            dialog.Commands.Add(new UICommand("Close"));
            if ((await dialog.ShowAsync()).Label != "Close")
                e.Handled = true;
            deferral.Complete();
        }
    }
}
