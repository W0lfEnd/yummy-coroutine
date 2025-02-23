using System;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YummyCoroutine.Runtime.Core;
using YummyCoroutine.Runtime.Extensions;
using YummyCoroutine.Runtime.WithResult;

namespace YummyCoroutine.Tests
{
    public class YCoroutineTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            YCoroutine.Init();
            YCoroutine.ClearCustomHandlers();
            YCoroutine.AddCustomHandler(new YCoroutineWithResultCustomHandler());
        }

        private IEnumerator SimpleExampleEnumerator(int framesToWait = 1)
        {
            for (int i = 0; i < framesToWait; i++)
                yield return null;
        }

        [UnityTest]
        public IEnumerator OnComplete_And_OnSuccess_Test()
        {
            bool onCompleteCalled = false;
            bool onSuccessCalled = false;

            YCoroutine cor = new();
            cor.Start(SimpleExampleEnumerator(1))
                .OnComplete(() => onCompleteCalled = true)
                .OnSuccess(() => onSuccessCalled = true);

            yield return null;
            Assert.IsTrue(onCompleteCalled, "OnComplete should be called when coroutine finishes.");
            Assert.IsTrue(onSuccessCalled, "OnSuccess should be called when coroutine completes without errors.");
        }

        [UnityTest]
        public IEnumerator Nested_Coroutines_Test()
        {
            var nestedCoroutineWaitFrames = 2;
            var mainCoroutineWaitFrames = 1;

            IEnumerator NestedCoroutine()
            {
                yield return SimpleExampleEnumerator(nestedCoroutineWaitFrames);
            }

            IEnumerator MainCoroutine()
            {
                yield return NestedCoroutine();
                yield return SimpleExampleEnumerator(mainCoroutineWaitFrames);
            }

            var isMainCoroutineFinished = false;
            MainCoroutine().Start()
                .OnComplete(() => isMainCoroutineFinished = true);

            //skip frames
            var totalFramesToWait = nestedCoroutineWaitFrames + mainCoroutineWaitFrames;
            for (int i = 0; i < totalFramesToWait; i++)
                yield return null;

            Assert.IsTrue(isMainCoroutineFinished, "Nested coroutines must finish in sequence.");
        }

        [UnityTest]
        public IEnumerator Stop_Coroutine_Test()
        {
            bool onStoppedCalled = false;

            YCoroutine cor = new();
            cor.Start(YCoroutineExtensions.While(() => true))
               .OnStopped(() => onStoppedCalled = true);

            //skip 2 frames
            for (int i = 0; i < 2; i++)
                yield return null;

            cor.Stop();

            yield return null;
            Assert.IsTrue(onStoppedCalled, "Stopping a coroutine should call OnStopped.");
        }

        [UnityTest]
        public IEnumerator Exception_Handling_Test()
        {
            const string expectedExceptionMsg = "Test exception 123321";
            var exception = new Exception(expectedExceptionMsg);
            string realMsg = "";
            bool onExceptionCalled = false;

            IEnumerator ExceptionEnumerator()
            {
                yield return null;
                throw exception;
            }

            LogAssert.Expect(LogType.Exception, new Regex($".*{expectedExceptionMsg}.*"));

            YCoroutine cor = new();
            cor.Start(ExceptionEnumerator())
               .OnException(ex =>
               {
                   onExceptionCalled = true;
                   realMsg = ex.Message;
               });

            yield return null;

            Assert.IsTrue(onExceptionCalled, "Coroutines should capture enumerator exceptions.");
            Assert.AreEqual(expectedExceptionMsg, realMsg);
        }

        [UnityTest]
        public IEnumerator Parallel_Usage_Test()
        {
            bool firstParallelFinished = false;
            bool secondParallelFinished = false;
            bool parallelHolderFinished = false;

            IEnumerator ParallelHolder()
            {
                yield return SimpleExampleEnumerator(1).Parallel()
                    .OnComplete(() => firstParallelFinished = true);

                yield return SimpleExampleEnumerator(1).Parallel()
                    .OnComplete(() => secondParallelFinished = true);

                parallelHolderFinished = true;
            }

            ParallelHolder().Start();

            Assert.IsFalse(firstParallelFinished, "Parallel coroutines should not finish before their holder.");
            Assert.IsFalse(secondParallelFinished, "Parallel coroutines should not finish before their holder.");

            yield return null;

            Assert.IsTrue(firstParallelFinished, "First parallel coroutine should finish.");
            Assert.IsTrue(secondParallelFinished, "Second parallel coroutine should finish.");
            Assert.IsTrue(parallelHolderFinished, "Parallel holder should finish after its coroutines.");
        }
    }
}