using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace ShortDev.Uwp.FullTrust;

public static class CoreDispatcherExtensions
{
    // https://github.com/Microsoft/Windows-task-snippets/blob/master/tasks/UI-thread-task-await-from-background-thread.md

    public static async Task<T> RunTaskAsync<T>(this CoreDispatcher @this, Func<Task<T>> callback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
    {
        TaskCompletionSource<T> taskCompletionSource = new();
        _ = @this.RunAsync(priority, async () =>
        {
            try
            {
                taskCompletionSource.SetResult(await callback());
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });
        return await taskCompletionSource.Task;
    }

    public static async Task RunTaskAsync(this CoreDispatcher @this, Func<Task> callback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        => await @this.RunTaskAsync(
                async () =>
                {
                    await callback();
                    return true;
                },
                priority
            );

    public static async Task RunTaskAsync(this CoreDispatcher @this, Action callback, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        => await @this.RunTaskAsync(
                () =>
                {
                    callback();
                    return Task.FromResult(true);
                },
                priority
            );
}
