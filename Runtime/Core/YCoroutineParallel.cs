using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YummyCoroutine.Runtime.Core
{
    public partial class YCoroutine
    {
        private List<YCoroutineParallel> _parallelCoroutines;

        public IYCoroutine Parallel(bool stopParent = true)
        {
            return new YCoroutineParallel(this, stopParent);
        }

        private void AddParallelCoroutine(YCoroutineParallel pc)
        {
            _parallelCoroutines ??= new List<YCoroutineParallel>();
            _parallelCoroutines.Add(pc);
            pc.OnSuccess(() => _parallelCoroutines.Remove(pc));
        }
        
        private IEnumerable<YCoroutineParallel> EnumerateActiveParallelCoroutines()
        {
            if (_parallelCoroutines == null)
                yield break;

            while (true)
            {
                var activeCoroutine = _parallelCoroutines.FirstOrDefault(it => !it.IsFinished);
                if (activeCoroutine == null)
                    yield break;
        
                yield return activeCoroutine;
            }
        }

        private bool NeedStopByParallel()
        {
            return _parallelCoroutines.Any(it => it.StopParent && it.State == YCoroutineState.Interrupted);
        }
    }

    public class YCoroutineParallel : IYCoroutine
    {
        public readonly bool StopParent;
        private readonly YCoroutine _parent;

        public YCoroutineParallel(YCoroutine yCoroutine, bool stopParent)
        {
            _parent = yCoroutine ?? throw new ArgumentNullException(nameof(yCoroutine));
            this.StopParent = stopParent;
        }

        public YCoroutineState State => _parent.State;
        public bool IsFinished => _parent.IsFinished;
        public bool IsPaused => _parent.IsPaused;
        public Exception Exception => _parent.Exception;

        public void SetFinished()
        {
            _parent.SetFinished();
        }

        public void Stop()
        {
            _parent.Stop();
        }

        public IYCoroutine OnComplete(Action action)
        {
            _parent.OnComplete(action);
            return this;
        }

        public IYCoroutine OnSuccess(Action action)
        {
            _parent.OnSuccess(action);
            return this;
        }

        public IYCoroutine OnStopped(Action action)
        {
            _parent.OnStopped(action);
            return this;
        }

        public IYCoroutine OnException(Action<Exception> action)
        {
            _parent.OnException(action);
            return this;
        }

        public IEnumerator WaitEnd(bool throwIfStopped = true)
        {
            return _parent.WaitEnd(throwIfStopped);
        }

        public IYCoroutine Parallel(bool stopParent = true)
        {
            return new YCoroutineParallel(_parent, stopParent);
        }
    }
}