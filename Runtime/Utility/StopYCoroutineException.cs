using System;
using YummyCoroutine.Runtime.Core;

namespace YummyCoroutine.Runtime.Utility
{
    public class StopYCoroutineException : Exception
    {
        public YCoroutine Coroutine;

        public StopYCoroutineException(YCoroutine coroutine = null)
        {
            this.Coroutine = coroutine;
        }
    }
}