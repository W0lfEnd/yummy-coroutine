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

        private IEnumerator WaitFramesCoroutine(int framesToWait = 1)
        {
            for (int i = 0; i < framesToWait; i++)
                yield return null;
        }

        [UnityTest]
        public IEnumerator OnComplete_Test()
        {
            bool onCompleteCalled = false;

            YCoroutine cor = WaitFramesCoroutine(1).Start()
                .OnComplete(() => onCompleteCalled = true);

            yield return null;
            Assert.IsTrue(onCompleteCalled, "OnComplete should be called when coroutine finishes no matter with error or not.");
        }

        [UnityTest]
        public IEnumerator OnSuccess_Test()
        {
            bool onSuccessCalled = false;

            YCoroutine cor = WaitFramesCoroutine(1).Start()
                .OnSuccess(() => onSuccessCalled = true);

            yield return null;
            Assert.IsTrue(onSuccessCalled, "OnSuccess should be called when coroutine completes without errors.");
        }

        [UnityTest]
        public IEnumerator Stop_Coroutine_Test()
        {
            bool onStoppedCalled = false;

            YCoroutine cor = new();
            cor.Start(YCoroutineExtensions.WhileCoroutine(() => true))
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
        public IEnumerator Pause_Coroutine_Test()
        {
            bool onPauseTrueCalled = false;
            bool onPauseFalseCalled = false;

            var cor = WaitFramesCoroutine(2).Start()
               .OnPause(isPaused =>
               {
                   onPauseTrueCalled |= isPaused;
                   onPauseFalseCalled |= !isPaused;
               });

            cor.IsPaused = true;

            //without pause coroutine will finish after 2 frames
            for (int i = 0; i < 10; i++)
                yield return null;

            Assert.IsTrue(onPauseTrueCalled, "Pausing a coroutine should call OnPause.");
            Assert.IsTrue(cor.IsPaused, "IsPaused should be true after pausing.");
            Assert.IsFalse(cor.IsFinished, "IsFinished should be false after pausing.");

            cor.IsPaused = false;

            for (int i = 0; i < 2; i++)
                yield return null;

            Assert.IsTrue(onPauseFalseCalled, "Resuming a coroutine should call OnPause.");
            Assert.IsFalse(cor.IsPaused, "IsPaused should be false after resuming.");
            Assert.IsTrue(cor.IsFinished, "IsFinished should be true after resuming.");
        }

        [UnityTest]
        public IEnumerator Nested_Coroutines_Test()
        {
            var nestedCoroutineWaitFrames = 2;

            IEnumerator NestedCoroutine()
            {
                yield return WaitFramesCoroutine(nestedCoroutineWaitFrames);
            }

            IEnumerator MainCoroutine()
            {
                yield return NestedCoroutine(); // yield returning IEnumerator object
                yield return NestedCoroutine().Start(); // yield returning YCoroutine object
            }

            var frameNumberBeforeStart = Time.frameCount;
            var cor = MainCoroutine().Start();

            yield return new WaitUntil(() => cor.IsFinished);

            var frameNumberAfterFinish = Time.frameCount;
            var framesPassed = frameNumberAfterFinish - frameNumberBeforeStart;

            Assert.AreEqual(nestedCoroutineWaitFrames * 2, framesPassed, "Nested coroutines must finish in sequence.");
        }

        [UnityTest]
        public IEnumerator Parallel_Coroutines_Test()
        {
            var parallelCoroutineWaitFrames = 2;

            IEnumerator SomeCoroutine()
            {
                yield return WaitFramesCoroutine(parallelCoroutineWaitFrames);
            }

            IEnumerator MainCoroutine()
            {
                yield return SomeCoroutine().Parallel();
                yield return SomeCoroutine().Start().Parallel();
            }

            var frameNumberBeforeStart = Time.frameCount;
            var cor = MainCoroutine().Start();

            yield return new WaitUntil(() => cor.IsFinished);

            var frameNumberAfterFinish = Time.frameCount;
            var framesPassed = frameNumberAfterFinish - frameNumberBeforeStart;

            Assert.AreEqual(parallelCoroutineWaitFrames, framesPassed, "Coroutines must finish in parallel.");
        }

        [UnityTest]
        public IEnumerator WithResult_Test()
        {
            var expectedResult = 123;

            IEnumerator WithResultCoroutine()
            {
                yield return WaitFramesCoroutine(10);
                yield return new YResult<int>(expectedResult); //can cause to InvalidCastException if type is invalid. Also will be skipped if parent YCoroutine is not a YCoroutineWithResult inheritor

                yield return WaitFramesCoroutine(10); //will never execute
                Debug.Log("Will never execute"); //will never execute
            }

            var frameNumberBeforeStart = Time.frameCount;
            var coroutineWithResult = new YCoroutineWithResult<int>().Start(WithResultCoroutine());

            yield return coroutineWithResult.WaitForResult; //can cause StopYCoroutineException if coroutine will end without success (stopped or exception)
            var frameNumberAfterFinish = Time.frameCount;
            var framesPassed = frameNumberAfterFinish - frameNumberBeforeStart;

            Assert.IsTrue(coroutineWithResult.HasResult, "Coroutine should have a result.");
            Assert.AreEqual(expectedResult, coroutineWithResult.Result, "Result should be set correctly.");

            Assert.AreEqual(10, framesPassed, "Coroutine should end after setting the result.");
        }
    }
}