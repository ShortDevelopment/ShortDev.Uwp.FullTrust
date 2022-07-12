using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using XamlApplication = Windows.UI.Xaml.Application;
using XamlElement = Windows.UI.Xaml.UIElement;

namespace ShortDev.Uwp.FullTrust.Xaml
{
    public sealed class XamlApplicationWrapper : IDisposable
    {
        public XamlApplication Application { get; private set; }

        public XamlApplicationWrapper(Func<XamlApplication> callback)
        {
            ThrowOnAlreadyRunning();

            Application = callback();
#pragma warning disable CS0612 // Type or member is obsolete
            Current = this;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public void Dispose()
        {
            Application = null;
        }

        internal static void ThrowOnAlreadyRunning()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (Current != null)
                throw new InvalidOperationException($"Only one instance of \"{nameof(XamlApplicationWrapper)}\" is allowed!");
#pragma warning restore CS0612 // Type or member is obsolete
        }

        [Obsolete]
        public static XamlApplicationWrapper? Current { get; private set; }

        [Obsolete]
        public static void Run<TApp, TContent>() where TApp : XamlApplication, new() where TContent : XamlElement, new()
            => Run<TApp, TContent>(null);

        [Obsolete]
        public static void Run<TApp, TContent>(Action? callback) where TApp : XamlApplication, new() where TContent : XamlElement, new()
        {
            using (XamlApplicationWrapper appWrapper = new(() => new TApp()))
            {
                string windowTitle = Process.GetCurrentProcess().ProcessName;
                try
                {
                    windowTitle = Package.Current?.DisplayName ?? windowTitle;
                }
                catch { }
                var window = XamlWindowActivator.CreateNewWindow(new(windowTitle));
                window.Content = new TContent();

                callback?.Invoke();

                // Run
                // XamlWindowSubclass.ForWindow(window).CurrentFrameworkView!.Run();
                window.Dispatcher.ProcessEvents(Windows.UI.Core.CoreProcessEventsOption.ProcessUntilQuit);
            }
        }
    }
}
