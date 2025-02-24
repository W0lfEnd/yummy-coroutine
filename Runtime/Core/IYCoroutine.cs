using System;
using System.Collections;

namespace YummyCoroutine.Runtime.Core
{
    public interface IYCoroutine
    {
        event Action onComplete;
        event Action onSuccess;
        event Action onStopped;
        event Action<Exception> onException;
        event Action<bool> onPause;

        IYCoroutine OnComplete(Action action);
        IYCoroutine OnSuccess(Action action);
        IYCoroutine OnStopped(Action action);
        IYCoroutine OnException(Action<Exception> action);
        IYCoroutine OnPause(Action<bool> action);

        YCoroutineState State { get; }
        bool IsFinished { get; }
        bool IsPaused { get; }

        Exception Exception { get; }

        void SetFinished();
        void Stop();
        IYCoroutine Parallel(bool stopParent = false);
        IEnumerator WaitEnd(bool throwIfStopped = true);
    }
}