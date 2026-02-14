using System.Runtime.CompilerServices;

namespace NRustFfi.Futures;

internal class FutureAwaiter<TFut, TPoll, TOut> :
    ICriticalNotifyCompletion,
    IDisposable
    where TFut : IFuture<TPoll, TOut>
    where TPoll : IPoll<TOut>
    where TOut : struct
{
    private readonly TFut _fut;
    private Context _context;
    private readonly nint _wakerPtr;

    private AsyncProperties _asyncProperties;

    private FutureAwaiter(TFut future)
    {
        using (ExecutionContext.SuppressFlow())
        {
            void onWake() => ThreadPool.UnsafeQueueUserWorkItem(static state => state.AdvanceFuture(), this, false);

            _fut = future;

            _wakerPtr = Waker.Create(onWake);

            _context = new Context
            {
                WakerPtr = _wakerPtr
            };
        }
    }

    internal static bool TryCompleteSynchronously(TFut future, CancellationToken token, out TOut? syncResult, out FutureAwaiter<TFut, TPoll, TOut>? asyncAwaiter)
    {
        syncResult = default;
        asyncAwaiter = null;

        var awaiter = new FutureAwaiter<TFut, TPoll, TOut>(future);
        var pollState = awaiter.Poll(out var resultValue);
        if (pollState == PollDiscriminant.Ready)
        {
            syncResult = resultValue;
            awaiter.Cleanup();
            return true;
        }
        else if (pollState == PollDiscriminant.Panicked)
        {
            awaiter.Cleanup();
            throw new Exception("A panic occurred while polling the future.");
        }

        awaiter.UpgradeToAsynchronous(token);
        asyncAwaiter = awaiter;
        return false;
    }

    private PollDiscriminant Poll(out TOut? resultValue)
    {
        resultValue = default;
        var result = _fut.Poll(ref _context);
        if (result.State == PollDiscriminant.Ready)
            resultValue = result.Value;

        return result.State;
    }

    private void UpgradeToAsynchronous(CancellationToken token)
    {
        _asyncProperties = new AsyncProperties
        {
            CapturedExecutionContext = GetExecutionContextWithoutSynchronizationContext(),
            LastPollState = PollDiscriminant.Pending,
            Token = token,
            TokenRegistration = token.Register(static state => ((FutureAwaiter<TFut, TPoll, TOut>)state!).ContinueWithCancellation(), this, useSynchronizationContext: false)
        };
    }

    private void AdvanceFuture()
    {
        lock (this)
        {
            if (_asyncProperties.Token.IsCancellationRequested)
            {
                // After cancellation, a scheduled wake callback may still run.
                return;
            }

            _asyncProperties.LastPollState = Poll(out var resultValue);
            switch (_asyncProperties.LastPollState)
            {
                case PollDiscriminant.Ready:
                    _asyncProperties.ResultValue = resultValue;
                    Continue();
                    break;
                case PollDiscriminant.Pending:
                    break;
                case PollDiscriminant.Panicked:
                    Continue();
                    break;
            }
        }
    }

    internal void ContinueWithCancellation()
    {
        lock (this)
        {
            Continue();
        }
    }

    private void Continue()
    {
        if (_asyncProperties.Continuation == null || _asyncProperties.ContinuationQueued)
            return;

        _asyncProperties.ContinuationQueued = true;

        if (!_asyncProperties.IsCleanedUp)
        {
            Cleanup();
            _asyncProperties.IsCleanedUp = true;
        }

        ThreadPool.UnsafeQueueUserWorkItem(static thisRef =>
            ExecutionContext.Run(thisRef._asyncProperties.CapturedExecutionContext, static state => ((FutureAwaiter<TFut, TPoll, TOut>)state!).CompleteAndRunContinuation(),
                thisRef), this, false);
    }

    private void Cleanup()
    {
        _fut.Drop();
        ref var waker = ref Waker.FromRawPointer(_wakerPtr);

        // Final drop to dispose of the original Waker instance which is constructed with ReferenceCount = 1.
        waker.Drop();
    }

    private void CompleteAndRunContinuation()
    {
        Volatile.Write(ref _asyncProperties.Completed, true);
        _asyncProperties.Continuation!();
    }

    public FutureAwaiter<TFut, TPoll, TOut> GetAwaiter()
    {
        return this;
    }

    public bool IsCompleted
    {
        get { return _asyncProperties.Token.IsCancellationRequested || _asyncProperties.LastPollState == PollDiscriminant.Panicked || Volatile.Read(ref _asyncProperties.Completed); }
    }

    public TOut? GetResult()
    {
        _asyncProperties.Token.ThrowIfCancellationRequested();

        if (_asyncProperties.LastPollState == PollDiscriminant.Panicked)
            throw new Exception("A panic occurred while polling the future.");

        return _asyncProperties.ResultValue;
    }

    public void OnCompleted(Action continuation)
    {
        throw new NotImplementedException();
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        // This method is called immediately after the constructor so callbacks may already fire, hence the lock.
        lock (this)
        {
            _asyncProperties.Token.ThrowIfCancellationRequested();
            _asyncProperties.Continuation = continuation;
            if (_asyncProperties.LastPollState is PollDiscriminant.Ready or PollDiscriminant.Panicked)
                Continue();
        }
    }

    public void Dispose() => _asyncProperties.TokenRegistration.Dispose();

    private static ExecutionContext GetExecutionContextWithoutSynchronizationContext()
    {
        var synchronizationContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        var executionContext = ExecutionContext.Capture()!;
        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        return executionContext;
    }

    private struct AsyncProperties
    {
        internal CancellationToken Token;
        internal CancellationTokenRegistration TokenRegistration;
        internal ExecutionContext CapturedExecutionContext;
        internal Action? Continuation;
        internal TOut? ResultValue;
        internal PollDiscriminant LastPollState;
        internal bool Completed;
        internal bool ContinuationQueued;
        internal bool IsCleanedUp;
    }
}
