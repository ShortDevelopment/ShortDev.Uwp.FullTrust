using System.Threading;
using Windows.Foundation;

namespace ShortDev.Uwp.FullTrust.Core.Xaml.Navigation
{
    public sealed class XamlWindowCloseRequestedEventArgs
    {
        XamlWindowSubclass _subclass;
        public XamlWindowCloseRequestedEventArgs(XamlWindowSubclass subclass)
            => _subclass = subclass;

        internal bool IsDeferred { get; private set; } = false;

        /// <summary>
        /// A <see cref="Deferral"/> object for the CloseRequested event.
        /// </summary>
        public Deferral GetDeferral()
        {
            IsDeferred = true;
            return new(() =>
            {
                if (!Handled)
                {
                    _subclass.CloseAllowed = true;
                    _subclass.Window.Close();
                }
            });
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the close request is handled by the app. <br />
        /// <see langword="true"/> if the app has handled the close request; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </summary>
        public bool Handled { get; set; } = false;
    }
}
