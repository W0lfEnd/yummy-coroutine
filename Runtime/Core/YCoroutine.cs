using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YummyCoroutine.Runtime.Utility;

namespace YummyCoroutine.Runtime.Core
{
    public partial class YCoroutine : IYCoroutine
    {
        public static YCoroutine FinishedCoroutine => new() { State = YCoroutineState.FinishedSuccessfully };
        public static YCoroutine StoppedCoroutine => new() { State = YCoroutineState.Interrupted };

        public YCoroutineState State { get; protected set; } = YCoroutineState.FinishedSuccessfully;
        public bool IsFinished => State is YCoroutineState.FinishedSuccessfully or YCoroutineState.Interrupted;
        public bool IsRunning => State == YCoroutineState.Running;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused == value)
                    return;

                _isPaused = value;
                onPause?.Invoke(value);
            }
        }
        public Exception Exception { get; protected set; }

        private static readonly HashSet<YCoroutine> _allActiveCoroutines = new();
        private Coroutine _coroutine;
        private bool _isPaused;

        public YCoroutine Start(IEnumerator enumerator)
        {
            if (!_coroutineCoroutiner)
            {
                Debug.LogError("Coroutine coroutiner is not initialized!");
                return StoppedCoroutine;
            }

            Stop();

            enumerator = StartInternal(enumerator);
            _coroutine = _coroutineCoroutiner.StartCoroutine(enumerator);

            _allActiveCoroutines.Add(this);

            return this;
        }

        public virtual void Stop()
        {
            if (!_coroutineCoroutiner)
            {
                Debug.LogError("Coroutine coroutiner is not initialized!");
                return;
            }

            if (IsFinished)
                return;

            if (_coroutine != null)
                _coroutineCoroutiner.StopCoroutine(_coroutine);

            if (_parallelCoroutines != null)
            {
                foreach (YCoroutineParallel parallelCoroutine in _parallelCoroutines)
                    parallelCoroutine.Stop();
            }

            State = YCoroutineState.Interrupted;
            DoFinishActions();
        }

        public void SetFinished()
        {
            if (IsFinished)
                return;

            State = YCoroutineState.FinishedSuccessfully;
            DoFinishActions();
        }

        public IEnumerator WaitEnd(bool throwIfStopped = true)
        {
            while (!IsFinished)
                yield return null;

            if (throwIfStopped && State is YCoroutineState.Interrupted)
                throw new StopYCoroutineException();
        }

        private IEnumerator StartInternal(IEnumerator enumerator)
        {
            State = YCoroutineState.Running;
            _isPaused = false;
            Exception = null;

            yield return EnumerateThrough(enumerator);

            if (_parallelCoroutines != null && _parallelCoroutines.Count > 0)
            {
                foreach (var pc in EnumerateActiveParallelCoroutines())
                    yield return EnumerateThrough(pc.WaitEnd(false));

                if (NeedStopByParallel())
                {
                    Stop();
                    yield break;
                }
            }

            State = YCoroutineState.FinishedSuccessfully;
            DoFinishActions();
        }

        private IEnumerator EnumerateThrough(IEnumerator enumerator)
        {
            while (!IsFinished)
            {
                while (!IsFinished && IsPaused)
                {
                    yield return null;
                }

                try
                {
                    if (!enumerator.MoveNext())
                        break;
                }
                catch(StopYCoroutineException)
                {
                    Stop();
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    StopWithException(ex);
                    break;
                }

                object yieldValue = enumerator.Current;
                switch (yieldValue)
                {
                    case YCoroutineParallel pc:
                        AddParallelCoroutine(pc);
                        break;

                    case YCoroutine mc:
                        yield return EnumerateThrough(mc.WaitEnd());
                        break;

                    case IEnumerator en:
                        yield return EnumerateThrough(en);
                        break;

                    default:
                    {
                        var handleResult = TryCustomHandle(yieldValue);
                        if (!handleResult.IsHandled)
                            yield return yieldValue;
                        else if (handleResult.NeedYieldReturn)
                            yield return handleResult.YieldValue;

                        break;
                    }
                }
            }
        }

        protected virtual void DoFinishActions()
        {
            _isPaused = false;
            onComplete?.Invoke();
            switch (State)
            {
                case YCoroutineState.FinishedSuccessfully:
                    onSuccess?.Invoke();
                    break;
                case YCoroutineState.Interrupted when Exception != null:
                    onException?.Invoke(Exception);
                    break;
                case YCoroutineState.Interrupted:
                    onStopped?.Invoke();
                    break;
                case YCoroutineState.Running:
                    Debug.LogError("Coroutine is running on finish actions!");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _parallelCoroutines?.Clear();
            _allActiveCoroutines.Remove(this);
        }

        private void StopWithException(Exception ex)
        {
            if (!_coroutineCoroutiner)
            {
                Debug.LogError("Coroutine coroutiner is not initialized!");
                return;
            }

            RememberException(ex);
            Stop();
        }

        private void RememberException(Exception exception)
        {
            Exception = exception;

            if (exception is StopYCoroutineException cse && cse.Coroutine == null)
                cse.Coroutine = this;
        }
    }
}