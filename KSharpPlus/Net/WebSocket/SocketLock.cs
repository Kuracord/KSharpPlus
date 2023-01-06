namespace KSharpPlus.Net.WebSocket; 

internal sealed class SocketLock : IDisposable {
    public SocketLock(ulong appId, int maxConcurrency) {
        ApplicationId = appId;
        TimeoutCancelSource = null!;
        MaxConcurrency = maxConcurrency;
        LockSemaphore = new SemaphoreSlim(maxConcurrency);
    }
    
    #region Fields and Properties

    public ulong ApplicationId { get; }

    SemaphoreSlim LockSemaphore { get; }
    CancellationTokenSource? TimeoutCancelSource { get; set; }
    CancellationToken TimeoutCancel => TimeoutCancelSource.Token;
    Task UnlockTask { get; set; }
    int MaxConcurrency { get; set; }

    #endregion

    #region Methods

    public async Task LockAsync() {
        await LockSemaphore.WaitAsync().ConfigureAwait(false);

        TimeoutCancelSource = new CancellationTokenSource();
        UnlockTask = Task.Delay(TimeSpan.FromSeconds(30), TimeoutCancel);
        UnlockTask.ContinueWith(InternalUnlock, TaskContinuationOptions.NotOnCanceled);
    }

    public void UnlockAfter(TimeSpan unlockDelay) {
        // it's not unlock-able because it's post-IDENTIFY or not locked
        if (TimeoutCancelSource == null || LockSemaphore.CurrentCount > 0) return;

        try {
            TimeoutCancelSource.Cancel();
            TimeoutCancelSource.Dispose();
        } catch {/**/}

        TimeoutCancelSource = null!;

        UnlockTask = Task.Delay(unlockDelay, CancellationToken.None);
        UnlockTask.ContinueWith(InternalUnlock);
    }

    public Task WaitAsync() => LockSemaphore.WaitAsync(TimeoutCancel);

    void InternalUnlock(Task t) => LockSemaphore.Release(MaxConcurrency);

    #endregion

    #region Utils

    public void Dispose() {
        try {
            TimeoutCancelSource.Cancel();
            TimeoutCancelSource.Dispose();
        } catch {/**/}
    }

    #endregion
}