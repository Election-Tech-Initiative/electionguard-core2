namespace ElectionGuard.Decryption.Concurrency;

/// <summary>
/// http://www.hanselman.com/blog/ComparingTwoTechniquesInNETAsynchronousCoordinationPrimitives.aspx 
/// </summary>
public sealed class SemaphoreAsyncLock : DisposableBase
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Task<IDisposable> _releaser;

    public SemaphoreAsyncLock()
    {
        _releaser = Task.FromResult((IDisposable)new Releaser(this));
    }

    public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        var wait = _semaphore.WaitAsync(cancellationToken);
        return wait.IsCompleted ?
                    _releaser :
                    wait.ContinueWith((_, state) => state as IDisposable,
                        _releaser.GetAwaiter().GetResult(), cancellationToken,
        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)!;
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        // TODO: dispose the releaser ore hold a handle to the cancellationToken
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreAsyncLock _toRelease;
        internal Releaser(SemaphoreAsyncLock toRelease) { _toRelease = toRelease; }
        public void Dispose() { _ = _toRelease._semaphore.Release(); }
    }
}

/// <summary>
/// Queue-based async lock with a Wait mechanism.
/// from: https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Concurrency/AsyncLock.cs
/// </summary>
public sealed class QueueAsyncLock : DisposableBase
{
    private bool _isAcquired;
    private bool _hasFaulted;
    private readonly object _guard = new object();
    private Queue<(Action<Delegate, object?> action, Delegate @delegate, object? state)>? _queue;

    /// <summary>
    /// Queues the action for execution. If the caller acquires the lock and becomes the owner,
    /// the queue is processed. If the lock is already owned, the action is queued and will get
    /// processed by the owner.
    /// </summary>
    /// <param name="action">Action to queue for execution.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public void Wait(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        Wait(action, static closureAction => closureAction());
    }

    /// <summary>
    /// Queues the action for execution. If the caller acquires the lock and becomes the owner,
    /// the queue is processed. If the lock is already owned, the action is queued and will get
    /// processed by the owner.
    /// </summary>
    /// <param name="action">Action to queue for execution.</param>
    /// <param name="state">The state to pass to the action when it gets invoked under the lock.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    /// <remarks>In case TState is a value type, this operation will involve boxing of <paramref name="state"/>.
    /// However, this is often an improvement over the allocation of a closure object and a delegate.</remarks>
    public void Wait<TState>(TState state, Action<TState> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        Wait(state, action, static (actionObject, stateObject) => ((Action<TState>)actionObject)((TState)stateObject!));
    }

    private void Wait(object? state, Delegate @delegate, Action<Delegate, object?> action)
    {
        // allow one thread to update the state
        lock (_guard)
        {
            // if a previous action crashed, ignore any future actions
            if (_hasFaulted)
            {
                return;
            }

            // if the "lock" is busy, queue up the extra work
            // otherwise there is no need to queue up "action"
            if (_isAcquired)
            {
                // create the queue if necessary
                var q = _queue;
                if (q == null)
                {
                    q = new Queue<(Action<Delegate, object?> action, Delegate @delegate, object? state)>();
                    _queue = q;
                }
                // enqueue the work
                q.Enqueue((action, @delegate, state));
                return;
            }

            // indicate there is processing going on
            _isAcquired = true;
        }

        // if we get here, execute the "action" first

        for (; ; )
        {
            try
            {
                action(@delegate, state);
            }
            catch
            {
                // the execution failed, terminate this AsyncLock
                lock (_guard)
                {
                    // throw away the queue
                    _queue = null;
                    // report fault
                    _hasFaulted = true;
                }
                throw;
            }

            // execution succeeded, let's see if more work has to be done
            lock (_guard)
            {
                var q = _queue;
                // either there is no queue yet or we run out of work
                if (q == null || q.Count == 0)
                {
                    // release the lock
                    _isAcquired = false;
                    return;
                }

                // get the next work action
                (action, @delegate, state) = q.Dequeue();
            }
            // loop back and execute the action
        }
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        lock (_guard)
        {
            _queue = null;
            _hasFaulted = true;
        }
    }
}

/// <summary>
/// http://www.hanselman.com/blog/ComparingTwoTechniquesInNETAsynchronousCoordinationPrimitives.aspx 
/// </summary>
public sealed class AsyncLock : DisposableBase
{
    private readonly SemaphoreAsyncLock _mutex;
    private readonly QueueAsyncLock _queue;

    public AsyncLock()
    {
        _mutex = new SemaphoreAsyncLock();
        _queue = new QueueAsyncLock();
    }

    public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        return _mutex.LockAsync(cancellationToken);
    }

    public void Wait(Action action)
    {
        _queue.Wait(action);
    }

    public void Wait<TState>(TState state, Action<TState> action)
    {
        _queue.Wait(state, action);
    }
}
