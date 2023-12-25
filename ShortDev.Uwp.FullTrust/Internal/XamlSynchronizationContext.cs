using System.Threading;
using Windows.UI.Core;

namespace ShortDev.Uwp.FullTrust.Internal;

internal sealed class XamlSynchronizationContext(CoreWindow coreWindow) : SynchronizationContext
{
    public CoreWindow CoreWindow { get; } = coreWindow;

    public override void Post(SendOrPostCallback d, object? state)
        => _ = CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => d?.Invoke(state));

    public override void Send(SendOrPostCallback d, object? state)
        => _ = CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => d?.Invoke(state));
}
