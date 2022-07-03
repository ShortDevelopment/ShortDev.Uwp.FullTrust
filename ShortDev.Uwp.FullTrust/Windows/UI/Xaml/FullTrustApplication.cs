﻿using ShortDev.Uwp.FullTrust.Core.Xaml;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.ViewManagement;

namespace Windows.UI.Xaml
{
    public sealed class FullTrustApplication
    {
        /// <summary>
        /// Gets the Application object for the current application.
        /// </summary>
        public static Application Current
            => Application.Current;

        /// <summary>
        /// Provides the entry point and requests initialization of the application. <br/>
        /// Use the callback to instantiate the Application class.
        /// </summary>
        /// <param name="callback">The callback that should be invoked during the initialization sequence.</param>
        [MTAThread]
        public static void Start([In] ApplicationInitializationCallback callback)
        {
            XamlApplicationWrapper.ThrowOnAlreadyRunning();

            Start(callback, XamlWindowConfig.Default);
        }

        /// <inheritdoc cref="Start(ApplicationInitializationCallback)" />
        /// <param name="windowConfig">Custom <see cref="XamlWindowConfig"/>.</param>
        [MTAThread]
        public static void Start([In] ApplicationInitializationCallback callback, [In] XamlWindowConfig windowConfig)
        {
            XamlApplicationWrapper.ThrowOnAlreadyRunning();

            Thread thread = CreateNewUIThread(() =>
            {
                // Application singleton is created here
                callback(null);

                // Satisfy our api
                _ = new XamlApplicationWrapper(() => Application.Current);

                // Create XamlWindow
                var window = XamlWindowActivator.CreateNewWindow(windowConfig);

                InvokeOnLaunched();

                // Run message loop
                XamlWindowSubclass.ForWindow(window).CurrentFrameworkView!.Run();
            });
            thread.Join();
        }

        /// <summary>
        /// Invokes <see cref="Application.OnLaunched(LaunchActivatedEventArgs)"/>
        /// </summary>
        static void InvokeOnLaunched()
        {
            var app = Current;
            app.GetType().GetMethod("OnLaunched", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, new[] { new Win32LaunchActivatedEventArgs() as object as LaunchActivatedEventArgs });

            return;
            IApplicationOverrides applicationOverrides = (IApplicationOverrides)app;
            applicationOverrides.OnLaunched(new Win32LaunchActivatedEventArgs());
        }

        sealed class Win32LaunchActivatedEventArgs : ILaunchActivatedEventArgs, IActivatedEventArgs, IApplicationViewActivatedEventArgs, IPrelaunchActivatedEventArgs, IViewSwitcherProvider, ILaunchActivatedEventArgs2, IActivatedEventArgsWithUser
        {
            User _currentUser;
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

            public ActivationViewSwitcher ViewSwitcher
                => null;

            public TileActivatedInfo TileActivatedInfo
                => null;

            public User User
                => _currentUser;
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

        /// <summary>
        /// Creates a new view for the app.
        /// </summary>
        public static CoreApplicationView CreateNewView()
            => CreateNewView(XamlWindowConfig.Default);

        /// ´<inheritdoc cref="CreateNewView" />
        public static CoreApplicationView CreateNewView(XamlWindowConfig windowConfig)
        {
            var result = XamlWindowActivator.CreateNewInternal(windowConfig);
            return result.coreAppView;
        }
    }
}
