using System;
using System.Collections;

namespace YummyCoroutine.Runtime.Core
{
    public interface IYCoroutine
    {
        YCoroutineState State { get; }
        bool IsFinished { get; }
        bool IsPaused { get; }

        Exception Exception { get; }

        void SetFinished();
        void Stop();
        IYCoroutine Parallel(bool stopParent = false);
        IEnumerator WaitEnd(bool throwIfStopped = true);

        IYCoroutine OnComplete(Action action);
        IYCoroutine OnSuccess(Action action);
        IYCoroutine OnStopped(Action action);
        IYCoroutine OnException(Action<Exception> action);
    }
}