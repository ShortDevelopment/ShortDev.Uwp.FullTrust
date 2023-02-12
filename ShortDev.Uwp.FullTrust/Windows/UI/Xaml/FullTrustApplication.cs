using ShortDev.Uwp.FullTrust.Xaml;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Markup;

namespace Windows.UI.Xaml;

public abstract class FullTrustApplication : Application, IXamlMetadataProvider
{
    /// <summary>
    /// Gets the Application object for the current application.
    /// </summary>
    public static new Application Current
        => Application.Current;

    /// <summary>
    /// Provides the entry point and requests initialization of the application. <br/>
    /// Use the callback to instantiate the Application class.
    /// </summary>
    /// <param name="callback">The callback that should be invoked during the initialization sequence.</param>
    [MTAThread]
    public static new void Start([In] ApplicationInitializationCallback callback)
    {
        ThrowOnAlreadyRunning();

        Start(callback, XamlWindowConfig.Default);
    }

    /// <inheritdoc cref="Start(ApplicationInitializationCallback)" />
    /// <param name="windowConfig">Custom <see cref="XamlWindowConfig"/>.</param>
    [MTAThread]
    public static void Start([In] ApplicationInitializationCallback callback, [In] XamlWindowConfig windowConfig)
    {
        ThrowOnAlreadyRunning();

        Thread thread = CreateNewUIThread(() =>
        {
            // Application singleton is created here
            callback(null);
            IsRunning = true;

            // Create XamlWindow
            var window = XamlWindowActivator.CreateNewWindow(windowConfig);

            InvokeOnLaunched();

            // Run message loop
            window.Dispatcher.ProcessEvents(UI.Core.CoreProcessEventsOption.ProcessUntilQuit);
        });
        thread.Join();
    }

    #region "InvokeOnLaunched"
    /// <summary>
    /// Invokes <see cref="Application.OnLaunched(LaunchActivatedEventArgs)"/>
    /// </summary>
    static void InvokeOnLaunched()
    {
        var app = Current;

        LaunchActivatedEventArgs? args = null;
        app.GetType().GetMethod("OnLaunched", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(app, new[] { args });
    }
    #endregion

    public static bool IsRunning { get; private set; } = false;
    static void ThrowOnAlreadyRunning()
    {
        if (IsRunning)
            throw new InvalidOperationException($"Only one instance of \"{nameof(Application)}\" is allowed!");
    }

    public static Thread CreateNewUIThread(Action callback)
    {
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        Thread thread = new(() => callback());
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return thread;
    }

    #region "CreateNewView"
    /// <summary>
    /// Creates a new view for the app.
    /// </summary>
    public static CoreApplicationView CreateNewView()
        => CreateNewView(XamlWindowConfig.Default);

    /// <inheritdoc cref="CreateNewView" />
    public static CoreApplicationView CreateNewView(XamlWindowConfig windowConfig)
    {
        CoreApplicationView? coreAppView = null;

        AutoResetEvent @event = new(false);
        CreateNewUIThread(() =>
        {
            var window = XamlWindowActivator.CreateNewWindowInternal(windowConfig, out coreAppView);

            @event.Set();

            // Run message loop
            window.Dispatcher.ProcessEvents(UI.Core.CoreProcessEventsOption.ProcessUntilQuit);
        });
        @event.WaitOne();

        return coreAppView!;
    }
    #endregion

    #region Implementation
    protected abstract IXamlMetadataProvider GetProvider();

    IXamlMetadataProvider? _appProvider = null;
    IXamlMetadataProvider AppProvider
        => _appProvider ??= GetProvider();

    public IXamlType GetXamlType(Type type)
        => AppProvider.GetXamlType(type);

    public IXamlType GetXamlType(string fullName)
        => AppProvider.GetXamlType(fullName);

    public XmlnsDefinition[] GetXmlnsDefinitions()
        => AppProvider.GetXmlnsDefinitions();
    #endregion
}
