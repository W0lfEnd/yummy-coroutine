using System;
using UnityEngine;
using YummyCoroutine.Runtime.Core;
using YummyCoroutine.Runtime.Utility;

namespace YummyCoroutine.Runtime.WithResult
{
    public class YCoroutineWithResult<T> : YCoroutine, IYResult
    {
        public event Action<T> onResult;

        public T Result { get; private set; }

        object IYResult.Result
        {
            get => Result;
            set
            {
                if (value is not T resultCasted)
                    throw new InvalidCastException($"Cannot cast {value.GetType()} to {typeof(T)}");

                Result = resultCasted;
                HasResult = true;
                SetFinished();
            }
        }

        public bool HasResult { get; private set; }

        public WaitUntil WaitForResult => new(() =>
        {
            if (State is YCoroutineState.Interrupted)
                throw new StopYCoroutineException();

            return HasResult;
        });

        public YCoroutineWithResult<T> OnResult(Action<T> action)
        {
            onResult = action;
            return this;
        }

        protected override void DoFinishActions()
        {
            base.DoFinishActions();

            if (State == YCoroutineState.Finished)
            {
                if (!HasResult)
                    Debug.LogError("Coroutine finished without a result!");

                onResult?.Invoke(Result);
            }
        }
    }
}