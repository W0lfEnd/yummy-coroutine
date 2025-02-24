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

        public static YCoroutine WithTimeout(this YCoroutine cor, float timeoutSeconds)
        {
            if (cor.State != YCoroutineState.FinishedSuccessfully)
                return cor;

            YCoroutine timeoutCor = Wait(timeoutSeconds).OnComplete(cor.Stop);
            cor.AddOnComplete(timeoutCor.Stop);
            return cor;
        }

        public static YCoroutine While(Func<bool> condition, object yieldReturn = null)
        {
            return new YCoroutine().Start(WhileCoroutine(condition, yieldReturn));
        }

        public static IEnumerator WhileCoroutine(Func<bool> condition, object yieldReturn = null)
        {
            while (condition())
                yield return yieldReturn;
        }

        public static YCoroutine WaitFrames(int delayFrames)
        {
            return new YCoroutine().Start(WaitFramesCoroutine(delayFrames));
        }
        
        public static YCoroutine Wait(float delay, bool unscaledTime = false)
        {
            return new YCoroutine().Start(WaitCoroutine(delay, unscaledTime));
        }

        private static IEnumerator WaitCoroutine(float delay, bool unscaledTime = true)
        {
            if (delay > 0.0f)
            {
                yield return unscaledTime
                    ? new WaitForSecondsRealtime(delay)
                    : new WaitForSeconds(delay);
            }
        }

        public static IEnumerator WaitFramesCoroutine(int delayFrames)
        {
            for (int i = 0; i < delayFrames; i++)
                yield return null;
        }

        public static void StopAndNull(ref YCoroutine cor)
        {
            cor?.Stop();
            cor = null;
        }
    }
}