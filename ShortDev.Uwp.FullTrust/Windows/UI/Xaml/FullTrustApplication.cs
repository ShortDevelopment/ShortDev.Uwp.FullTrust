using ShortDev.Uwp.FullTrust.Xaml;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Xaml.Markup;
using Windows.Win32.Foundation;

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

    // https://github.com/CommunityToolkit/Microsoft.Toolkit.Win32/blob/6fb2c3e00803ea563af20f6bc9363091b685d81f/Microsoft.Toolkit.Win32.UI.XamlApplication/XamlApplication.cpp#L140C5-L150
    static readonly string[] preloadDlls = new[] {
        "twinapi.appcore.dll",
        "threadpoolwinrt.dll",
    };

    [DllImport("CoreMessaging.dll", ExactSpelling = true, EntryPoint = "CreateDispatcherQueueController")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern HRESULT CreateDispatcherQueueController_Net3(Win32.System.WinRT.DispatcherQueueOptions options, out DispatcherQueueController dispatcherQueueController);

    /// <inheritdoc cref="Start(ApplicationInitializationCallback)" />
    /// <param name="windowConfig">Custom <see cref="XamlWindowConfig"/>.</param>
    [MTAThread]
    public static void Start([In] ApplicationInitializationCallback callback, [In] XamlWindowConfig windowConfig)
    {
        ThrowOnAlreadyRunning();

        List<HINSTANCE> preloadInstances = new();
        foreach (var lib in preloadDlls)
        {
            var instance = LoadLibraryEx(lib, default, 0);
            preloadInstances.Add(instance);
        }

        // Application singleton is created here
        callback(null);
        IsRunning = true;

        //Marshal.ThrowExceptionForHR(
        //    CreateDispatcherQueueController_Net3(new()
        //    {
        //        dwSize = (uint)Marshal.SizeOf<Win32.System.WinRT.DispatcherQueueOptions>(),
        //        apartmentType = Win32.System.WinRT.DISPATCHERQUEUE_THREAD_APARTMENTTYPE.DQTAT_COM_ASTA,
        //        threadType = Win32.System.WinRT.DISPATCHERQUEUE_THREAD_TYPE.DQTYPE_THREAD_CURRENT
        //    }, out var dispatcherController)
        //);

        try
        {
            // Create XamlWindow
            XamlWindowActivator.CreateNewWindowInternal(windowConfig, out _, out var frameworkView);

            InvokeOnLaunched();

            // Run message loop
            frameworkView.Run();
            frameworkView.Uninitialize();
        }
        finally
        {
            //dispatcherController.ShutdownQueueAsync().GetAwaiter().GetResult();
        }

        //foreach (var instance in preloadInstances)
        //    FreeLibrary(instance);
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
            var window = XamlWindowActivator.CreateNewWindowInternal(windowConfig, out coreAppView, out var frameworkView);

            @event.Set();

            // Run message loop
            frameworkView.Run();
            frameworkView.Uninitialize();
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
