using System;
using System.Collections;
using UnityEngine;
using YummyCoroutine.Runtime.Core;

namespace YummyCoroutine.Runtime.Extensions
{
    public static class YCoroutineExtensions
    {
        public static IYCoroutine Parallel(this IEnumerator enumerator, bool stopParent = true)
        {
            return enumerator.Start().Parallel(stopParent);
        }

        public static YCoroutine Start(this IEnumerator enumerator)
        {
            return new YCoroutine().Start(enumerator);
        }

        public static YCoroutine WithTimeout(this YCoroutine cor, float timeoutSeconds = 15.0f)
        {
            if (cor.State != YCoroutineState.Finished)
                return cor;

            YCoroutine timeoutCor = WaitAndDo(cor.Stop, timeoutSeconds);
            return cor.OnComplete(() => timeoutCor.Stop());
        }
        
        public static IEnumerator While(Func<bool> condition, object yieldReturn = null)
        {
            while (condition())
                yield return yieldReturn;
        }

        public static YCoroutine WaitAndDo(
            Action action
            , float delay = 0.0f
            , int delayFrames = 0
            , bool unscaledTime = false
        )
        {
            if (delay == 0 && delayFrames == 0)
            {
                action();
                return YCoroutine.FinishedCoroutine;
            }

            return new YCoroutine().Start(WaitAndDo(delay, action, delayFrames, unscaledTime));
        }

        public static IEnumerator WaitAndDo(float delay = 0.0f, Action callback = null, int delayFrames = 0, bool unscaledTime = true)
        {
            if (delay > 0.0f)
            {
                yield return unscaledTime
                    ? new WaitForSecondsRealtime(delay)
                    : new WaitForSeconds(delay);
            }

            for (int i = 0; i < delayFrames; i++)
                yield return null;

            callback?.Invoke();
        }

        public static YCoroutine WaitUntilAndDo(Func<bool> condition, Action callback = null, object returnObject = null)
        {
            if (condition())
            {
                callback?.Invoke();
                return YCoroutine.FinishedCoroutine;
            }

            IEnumerator Coroutine()
            {
                while (!condition())
                    yield return returnObject;

                callback?.Invoke();
            }

            return Coroutine().Start();
        }

        public static void StopAndNull(ref YCoroutine yCoroutine)
        {
            yCoroutine?.Stop();
            yCoroutine = null;
        }
    }
}