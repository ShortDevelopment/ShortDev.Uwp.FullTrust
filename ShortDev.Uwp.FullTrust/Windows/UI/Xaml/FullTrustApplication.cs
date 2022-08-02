using ShortDev.Uwp.FullTrust.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using ShortDev.Uwp.FullTrust;
using System.Collections.Generic;

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

        class C
        {
            public static void M()
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Invokes <see cref="Application.OnLaunched(LaunchActivatedEventArgs)"/>
        /// </summary>
        static unsafe void InvokeOnLaunched()
        {
            IntPtr pApplicationOverrides;
            IntPtr* vtable;
            {
                IntPtr pUnk = Marshal.GetIUnknownForObject(Current);
                Guid iid = typeof(IApplicationOverrides).GUID;
                Marshal.ThrowExceptionForHR(Marshal.QueryInterface(pUnk, ref iid, out pApplicationOverrides));
                vtable = *(IntPtr**)pApplicationOverrides;
            }

            var onLaunched = Marshal.GetDelegateForFunctionPointer<OnLaunchedProc>(vtable[7]);
            Win32LaunchActivatedEventArgs launchArgs = new();
            RCW rcw = new(Marshal.GetIUnknownForObject(launchArgs));
            Marshal.ThrowExceptionForHR(onLaunched(pApplicationOverrides, rcw.GetIUnkown()));
            GC.KeepAlive(launchArgs);
        }

        [ComVisible(true), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate int OnLaunchedProc([In] IntPtr @this, IntPtr args);

        unsafe class RCW : IDisposable
        {
            #region HRESULT
            const int S_OK = 0;
            const int E_POINTER = -2147467261;
            const int E_NOINTERFACE = unchecked((int)0x80004002);
            #endregion

            static readonly Guid IID_IUnkown = new("00000000-0000-0000-C000-000000000046");
            static readonly Guid IID_IInspectable = new("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90");

            public IntPtr PUnk { get; }

            public RCW(IntPtr pUnk)
            {
                PUnk = pUnk;
            }

            public int QueryInterface(IntPtr @this, ref Guid riid, void** ppv)
            {
                if ((IntPtr)ppv == IntPtr.Zero)
                    return E_POINTER;

                // *ppv = (void*)IntPtr.Zero;

                if (riid == IID_IUnkown)
                {
                    VtableRef vtable;
                    if (!_ppvs.TryGetValue(riid, out vtable))
                    {
                        vtable = new(3);
                        GenerateIUnkownVtable(vtable.Write());
                        _ppvs.Add(riid, vtable);
                    }
                    *ppv = (void*)vtable.Ptr;
                    return S_OK;
                }

                var hr = Marshal.QueryInterface(PUnk, ref riid, out var result);
                *ppv = (void*)result;
                return hr;
            }

            public IntPtr GetIUnkown()
            {
                IntPtr pv = IntPtr.Zero;
                Guid iid = IID_IUnkown;
                QueryInterface(IntPtr.Zero, ref iid, (void**)&pv);
                return pv;
            }

            IntPtr[]? _unkVtable;
            void GenerateIUnkownVtable(Span<IntPtr> vtable)
            {
                bool generate = _unkVtable == null;
                _unkVtable = _unkVtable ?? new IntPtr[3];

                if (generate)
                {
                    _unkVtable[0] = Marshal.GetFunctionPointerForDelegate(DelegateHelpers.CreateDelegate(this, typeof(RCW).GetMethod("QueryInterface")));
                    _unkVtable[1] = Marshal.GetFunctionPointerForDelegate(DelegateHelpers.CreateDelegate(this, typeof(RCW).GetMethod("AddRef")));
                    _unkVtable[2] = Marshal.GetFunctionPointerForDelegate(DelegateHelpers.CreateDelegate(this, typeof(RCW).GetMethod("Release")));
                }

                vtable[0] = _unkVtable[0]; // QueryInterface
                vtable[1] = _unkVtable[1]; // AddRef
                vtable[2] = _unkVtable[2]; // Release
            }

            #region Livetime
            public int RefCount { get; private set; } = 0;
            public int AddRef(IntPtr @this)
            {
                //RefCount++;
                return S_OK;
            }

            public int Release(IntPtr @this)
            {
                //RefCount--;
                //if (RefCount == 0)
                //    Dispose();
                return S_OK;
            }
            #endregion

            class VtableRef : IDisposable
            {
                public VtableRef(int size)
                {
                    Size = size;
                    Ptr = Marshal.AllocHGlobal(IntPtr.Size);
                    ContentPtr = Marshal.AllocHGlobal(IntPtr.Size * size);
                    Marshal.WriteIntPtr(Ptr, ContentPtr);
                }

                public IntPtr Ptr { get; private set; }
                public IntPtr ContentPtr { get; private set; }
                public int Size { get; private set; }

                public Span<IntPtr> Write()
                    => new Span<IntPtr>((void*)ContentPtr, Size);

                public void Dispose()
                {
                    Marshal.FreeHGlobal(ContentPtr);
                    Marshal.FreeHGlobal(Ptr);
                }
            }

            Dictionary<Guid, VtableRef> _ppvs = new();
            public bool IsDisposed = false;
            public void Dispose()
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("RCW");

                foreach (var ppv in _ppvs)
                    ppv.Value.Dispose();

                GC.KeepAlive(this);
            }
        }

        [ComVisible(true), ComDefaultInterface(typeof(ILaunchActivatedEventArgs))]
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
    }
}
