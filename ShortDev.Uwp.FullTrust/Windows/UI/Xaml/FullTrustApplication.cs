using ShortDev.Uwp.FullTrust.Xaml;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Markup;

namespace Windows.UI.Xaml
{
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
                XamlWindowSubclass.ForWindow(window).CurrentFrameworkView!.Run();
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

            Win32LaunchActivatedEventArgs args_0 = new();
            LaunchActivatedEventArgs args = args_0 as object as LaunchActivatedEventArgs;
            app.GetType().GetMethod("OnLaunched", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(app, new[] { args });

            return;
            IApplicationOverrides applicationOverrides = (IApplicationOverrides)app;
            applicationOverrides.OnLaunched(new Win32LaunchActivatedEventArgs());
        }

        sealed class Win32LaunchActivatedEventArgs : ILaunchActivatedEventArgs, IActivatedEventArgs, IApplicationViewActivatedEventArgs, IPrelaunchActivatedEventArgs, IViewSwitcherProvider, ILaunchActivatedEventArgs2, IActivatedEventArgsWithUser
        {
            User? _currentUser;
            public Win32LaunchActivatedEventArgs()
            {
                var result = User.FindAllAsync().GetAwaiter().GetResult();
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            static extern string GetCommandLineW();

            public string Arguments
                => GetCommandLineW();

            public string TileId
                => throw new NotImplementedException();

            public ActivationKind Kind
                => ActivationKind.Launch;

            public ApplicationExecutionState PreviousExecutionState
                => ApplicationExecutionState.NotRunning;

            public SplashScreen SplashScreen
                => throw new NotImplementedException();

            public int CurrentlyShownApplicationViewId
                => ApplicationView.GetForCurrentView().Id;

            public bool PrelaunchActivated
                => false;

            public ActivationViewSwitcher? ViewSwitcher
                => null;

            public TileActivatedInfo? TileActivatedInfo
                => null;

            public User? User
                => _currentUser;
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
                var result = XamlWindowActivator.CreateNewInternal(windowConfig);
                coreAppView = result.coreAppView;

                @event.Set();

                // Run message loop
                XamlWindowSubclass.ForWindow(result.window).CurrentFrameworkView!.Run();
            });
            @event.WaitOne();

            return coreAppView!;
        }
        #endregion

        #region Implementation
        protected abstract IXamlMetadataProvider GetProvider();

        IXamlMetadataProvider? __appProvider = null;
        IXamlMetadataProvider _AppProvider
        {
            get
            {
                if(__appProvider == null)
                    __appProvider = GetProvider();
                return __appProvider;
            }
        }

        public IXamlType GetXamlType(Type type)
            => _AppProvider.GetXamlType(type);

        public IXamlType GetXamlType(string fullName)
            => _AppProvider.GetXamlType(fullName);

        public XmlnsDefinition[] GetXmlnsDefinitions()
            => _AppProvider.GetXmlnsDefinitions();
        #endregion
    }
}
