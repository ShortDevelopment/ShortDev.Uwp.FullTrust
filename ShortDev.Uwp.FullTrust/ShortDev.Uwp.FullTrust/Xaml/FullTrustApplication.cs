using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.Win32.Foundation;
using WinRT;

namespace ShortDev.Uwp.FullTrust.Xaml;

public abstract class FullTrustApplication : Application, IXamlMetadataProvider
{
    // https://github.com/CommunityToolkit/Microsoft.Toolkit.Win32/blob/6fb2c3e00803ea563af20f6bc9363091b685d81f/Microsoft.Toolkit.Win32.UI.XamlApplication/XamlApplication.cpp#L140C5-L150
    static readonly List<HINSTANCE> _preloadInstances = new();
    static FullTrustApplication()
    {
        if (InteropHelper.IsAppContainer)
            return;

        foreach (var lib in new[] { "twinapi.appcore.dll", "threadpoolwinrt.dll", })
        {
            var instance = LoadLibraryEx(lib, 0);
            _preloadInstances.Add(instance);
        }
    }

    ~FullTrustApplication()
    {
        foreach (var instance in _preloadInstances)
            FreeLibrary(instance);
    }

    /// <summary>
    /// Provides the entry point and requests initialization of the application. <br/>
    /// Use the callback to instantiate the Application class.
    /// </summary>
    /// <param name="windowConfig">Custom <see cref="XamlWindowConfig"/>.</param>
    [MTAThread]
    public static void Start([In] ApplicationInitializationCallback callback, XamlWindowConfig? windowConfig = null)
    {
        if (InteropHelper.IsAppContainer)
        {
            Application.Start(callback);
            return;
        }

        ThrowOnAlreadyRunning();

        IActivatedEventArgs? args = null;
        if (InteropHelper.HasPackageIdentity)
        {
            args = AppInstance.GetActivatedEventArgs();
        }

        // Application singleton is created here
        callback(null);
        IsRunning = true;

        //Marshal.ThrowExceptionForHR(
        //    CreateDispatcherQueueController(new()
        //    {
        //        dwSize = (uint)Marshal.SizeOf<Win32.System.WinRT.DispatcherQueueOptions>(),
        //        apartmentType = Win32.System.WinRT.DISPATCHERQUEUE_THREAD_APARTMENTTYPE.DQTAT_COM_ASTA,
        //        threadType = Win32.System.WinRT.DISPATCHERQUEUE_THREAD_TYPE.DQTYPE_THREAD_CURRENT
        //    }, out var dispatcherController)
        //);

        try
        {
            windowConfig ??= XamlWindowConfig.Default;

            // Create XamlWindow
            XamlWindowActivator.CreateNewWindowInternal(windowConfig, out _, out var frameworkView);

            OnAppActivated(args);

            // Run message loop
            frameworkView.Run();
            frameworkView.Uninitialize();
        }
        finally
        {
            //dispatcherController.ShutdownQueueAsync().GetAwaiter().GetResult();
        }
    }

    static void OnAppActivated(IActivatedEventArgs? args)
    {
        var app = Current.As<IApplicationOverrides>();
        app.OnActivated(args);

        if (args == null)
        {
            app.OnLaunched(args: null);
            return;
        }

        // https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/get-activation-info-for-packaged-apps#supported-activation-types
        switch (args.Kind)
        {
            case ActivationKind.ShareTarget:
                app.OnShareTargetActivated(args.As<ShareTargetActivatedEventArgs>());
                break;
            case ActivationKind.File:
                app.OnFileActivated(args.As<FileActivatedEventArgs>());
                break;
            case ActivationKind.Launch:
                app.OnLaunched(args.As<LaunchActivatedEventArgs>());
                break;
        }
    }

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
