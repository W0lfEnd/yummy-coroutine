using System;

namespace YummyCoroutine.Runtime.Core
{
    public partial class YCoroutine
    {
        public event Action onComplete;
        public event Action onSuccess;
        public event Action onStopped;
        public event Action<Exception> onException;

        IYCoroutine IYCoroutine.OnComplete(Action action)
            => OnComplete(action);

        IYCoroutine IYCoroutine.OnSuccess(Action action)
            => OnSuccess(action);

        IYCoroutine IYCoroutine.OnStopped(Action action)
            => OnStopped(action);

        IYCoroutine IYCoroutine.OnException(Action<Exception> action)
            => OnException(action);

        public YCoroutine OnComplete(Action action)
        {
            onComplete = action;
            return this;
        }

        public YCoroutine OnSuccess(Action action)
        {
            onSuccess = action;
            return this;
        }

        public YCoroutine OnStopped(Action action)
        {
            onStopped = action;
            return this;
        }

        public YCoroutine OnException(Action<Exception> action)
        {
            onException = action;
            return this;
        }
    }
}