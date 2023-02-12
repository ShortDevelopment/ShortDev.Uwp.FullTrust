using ShortDev.Uwp.FullTrust.Xaml;
using ShortDev.Win32;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpUI
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Win32WindowSubclass.GetForCurrentView().SetTitleBar(titleBarEle);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeChoices.Add("Bild", new List<string>() { ".jpeg" });

            _ = picker.PickSaveFileAsync();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            filePicker.FileTypeFilter.Add(".mp3");
            filePicker.FileTypeFilter.Add(".wav");
            filePicker.FileTypeFilter.Add(".wma");
            filePicker.FileTypeFilter.Add(".m4a");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            _ = filePicker.PickSingleFileAsync();
        }

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            var view = FullTrustApplication.CreateNewView();
            _ = view.CoreWindow.Dispatcher.RunIdleAsync((x) =>
            {
                Window.Current.Content = new MainPage();
                Window.Current.Activate();
            });
        }
    }
}
