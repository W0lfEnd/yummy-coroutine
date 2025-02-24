using System;

namespace YummyCoroutine.Runtime.Core
{
    public partial class YCoroutine
    {
        public event Action onComplete;
        public event Action onSuccess;
        public event Action onStopped;
        public event Action<Exception> onException;
        public event Action<bool> onPause;

        IYCoroutine IYCoroutine.OnComplete(Action action)
            => OnComplete(action);

        IYCoroutine IYCoroutine.OnSuccess(Action action)
            => OnSuccess(action);

        IYCoroutine IYCoroutine.OnStopped(Action action)
            => OnStopped(action);

        IYCoroutine IYCoroutine.OnException(Action<Exception> action)
            => OnException(action);

        IYCoroutine IYCoroutine.OnPause(Action<bool> action)
            => OnPause(action);

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

        public YCoroutine OnPause(Action<bool> action)
        {
            onPause = action;
            return this;
        }

        public YCoroutine AddOnComplete(Action action)
        {
            onComplete += action;
            return this;
        }

        public YCoroutine AddOnSuccess(Action action)
        {
            onSuccess += action;
            return this;
        }

        public YCoroutine AddOnStopped(Action action)
        {
            onStopped += action;
            return this;
        }

        public YCoroutine AddOnException(Action<Exception> action)
        {
            onException += action;
            return this;
        }

        public YCoroutine AddOnPause(Action<bool> action)
        {
            onPause += action;
            return this;
        }

        public YCoroutine RemoveOnComplete(Action action)
        {
            onComplete -= action;
            return this;
        }
        
        public YCoroutine RemoveOnSuccess(Action action)
        {
            onSuccess -= action;
            return this;
        }
        
        public YCoroutine RemoveOnStopped(Action action)
        {
            onStopped -= action;
            return this;
        }
        
        public YCoroutine RemoveOnException(Action<Exception> action)
        {
            onException -= action;
            return this;
        }

        public YCoroutine RemoveOnPause(Action<bool> action)
        {
            onPause -= action;
            return this;
        }
    }
}