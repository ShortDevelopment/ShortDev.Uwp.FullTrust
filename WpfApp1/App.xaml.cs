using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Windows.UI.Xaml.Application singleton
            new UwpUI.App();
        }
    }
}
