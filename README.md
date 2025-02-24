# YummyCoroutine

Library for Unity that simplifies working with coroutines, adding convenient control methods, exception handling, and parallel execution.

## Content
- [**Getting Started**](#getting-started)
    - [Initialization](#initialization)
- [**Core Features**](#core-features)
    - [Starting Coroutines](#starting-coroutines)
    - [Nested Coroutines](#nested-coroutines)
    - [Completion Handling](#coroutine-completion-handling)
    - [Parallel Coroutine Execution](#parallel-coroutine-execution-parallel)
- [**YCoroutine with result**](#ycoroutine-with-result)
- [**YCoroutineExtensions**](#ycoroutineextensions)

## Getting Started

### Initialization

Before using **YummyCoroutine**, it needs to be initialized once in your project. This is typically done at the start of your application or in the `Awake()` method:

```csharp
public void Awake()
{
    YCoroutine.Init();
}
```

That code will create GameObject + MonoBehaviour which will be root for all YCoroutines.

## Core Features

### Starting Coroutines
You can use 2 ways to start YCoroutine, they are equal and each of them returns a YCoroutine object, which allows you to control the coroutine:
1. Use the ```.Start()``` extension method for IEnumerator.
```csharp
IEnumerator CoroutineImpl()
{
    Debug.Log("Coroutine started");
    yield return new WaitForSeconds(1f);
    Debug.Log("Coroutine finished");
}

YCoroutine yCor = CoroutineImpl().Start();

```
2. Use method ```.Start(IEnumerator enumerator)``` on YCoroutine object.
> it will stop previous coroutine contained in this YCoroutine object if it was existed
```csharp
YCoroutine cor = new YCoroutine();
cor.Start(CoroutineImpl());
```

### Nested Coroutines

**YummyCoroutine** supports nested coroutines. This means you can start one coroutine from within another. Inner coroutines will be executed **sequentially**, and the outer coroutine will only resume execution after all its nested coroutines have completed.

This allows you to create more complex sequences of asynchronous operations where one action depends on the completion of another.

**Example:**

Consider the example of nested coroutines, where `MainCoroutine` starts `NestedCoroutine`, and both coroutines simulate waiting for a certain number of frames:

```csharp
// Simple coroutine method used for examples bellow
public static IEnumerator WaitFramesCoroutine(int delayFrames)
{
    for (int i = 0; i < delayFrames; i++)
        yield return null;
}
```

```csharp
[UnityTest]
public IEnumerator Nested_Coroutines_Test1()
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
```
> Nested coroutines must be of type YCoroutine or IEnumerator

### Coroutine Completion Handling
YummyCoroutine provides few methods to handle coroutine completition:
1. ```.OnComplete(Action callback)```
2. ```.OnSuccess(Action callback)```
3. ```.OnStop(Action callback)```
4. ```.OnPause(Action<bool> callback)```
5. ```.OnException(Action<Exception> callback)```

All of them rewrites old ```callback```. 

To append new ```callback``` without erasing old one you can use events (```onComplete```, ```onSuccess```, etc) or methods (```AddOnComplete```, ```AddOnStop```, etc)

Now all ```callback``` methods in details:
* ```.OnComplete(Action callback)```: Calls the specified ```callback``` always after the coroutine finishes, regardless of whether it completed successfully, with an exception, or was stopped.

```csharp
[UnityTest]
public IEnumerator OnComplete_Test()
{
    bool onCompleteCalled = false;

    YCoroutine cor = WaitFramesCoroutine(1).Start()
        .OnComplete(() => onCompleteCalled = true);

    yield return null;
    Assert.IsTrue(onCompleteCalled, "OnComplete should be called when coroutine finishes no matter with error or not.");
}
```

* ```.OnSuccess(Action callback)```: Calls the specified ```callback``` only if the coroutine completed successfully, i.e., without throwing exceptions and was not stopped.

```csharp
[UnityTest]
public IEnumerator OnSuccess_Test()
{
    bool onSuccessCalled = false;

    YCoroutine cor = WaitFramesCoroutine(1).Start()
        .OnSuccess(() => onSuccessCalled = true);

    yield return null;
    Assert.IsTrue(onSuccessCalled, "OnSuccess should be called when coroutine completes without errors.");
}
```

* ```.OnStopped(Action callback)```: Calls the specified ```callback``` after the coroutine has been stopped by the ```.Stop()``` method.
```csharp
[UnityTest]
public IEnumerator OnStopped_Test()
{
    bool onStoppedCalled = false;

    YCoroutine cor = InfiniteLoopCoroutine().Start()
        .OnStopped(() => onStoppedCalled = true);

    yield return null;
    cor.Stop();

    Assert.IsTrue(onStoppedCalled, "OnStopped should be called when coroutine is stopped.");

    IEnumerator InfiniteLoopCoroutine(object yieldReturn = null)
    {
        while (true)
            yield return yieldReturn;
    }
}
```
* ```.OnPause(Action<bool> callback)```: Calls the specified ```callback``` if pause state was changed.
```csharp
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
```

* ```.OnException(Action<Exception> callback)```: Calls the specified ```callback``` if an exception is thrown inside the coroutine. The exception is passed as an argument to the ```callback```.
```csharp
[UnityTest]
public IEnumerator Exception_Handling_Test()
{
    IEnumerator ExceptionEnumerator()
    {
        yield return null;
        throw new Exception("Test exception 123321");
    }

    YCoroutine cor = new();
    cor.Start(ExceptionEnumerator())
       .OnException(ex =>
       {
          Debug.Log("Exception handled");
       });

    yield break;
}
```

### Parallel Coroutine Execution (Parallel)
YummyCoroutine allows you to run coroutines in parallel using the ```.Parallel()``` extension method. Coroutines started with ```.Parallel()``` execute independently, but their execution is coordinated by the owner coroutine. The owner coroutine will finish execution only after all parallel coroutines are completed.

```csharp
[UnityTest]
public IEnumerator Parallel_Coroutines_Test()
{
    var parallelCoroutineWaitFrames = 2;

    IEnumerator SomeCoroutine()
    {
        yield return WaitForFrames(parallelCoroutineWaitFrames);
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
```
## Custom yield value handler
You can create custom yield value handlers for YCoroutine. At the moment its only one: ```YCoroutineWithResultCustomHandler```
```csharp
public struct YCoroutineWithResultCustomHandler : IYCoroutineObjectHandler
{
    public bool CanHandle(object obj)
    {
        return obj is IYResult;
    }

    public void Handle(IYCoroutine coroutine, object obj)
    {
        if (coroutine is not IYResult coroutineWithResult)
            return;

        if (obj is not IYResult result)
            return;

        coroutineWithResult.Result = result.Result;
    }

    public bool CanReturnYieldValue => false;
    public object NewYieldValue => null;
}
```

and you should add it to handlers list using next code

```csharp
YCoroutine.AddCustomHandler(new YCoroutineWithResultCustomHandler());
```
## YCoroutine with result
```csharp
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
```

## YCoroutineExtensions

1. ```WithTimeout(this YCoroutine cor, float timeoutSeconds)```: Adds a timeout to an already running YCoroutine, stopping it if it does not complete within the specified time.
2. ```While(Func<bool> condition, object yieldReturn)```: Creates and starts a YCoroutine that executes in a loop as long as the given condition remains true.
3. ```WaitFrames(int delayFrames)```: Creates and starts a YCoroutine that waits specified number of frames
4. ```Wait(float delay, bool unscaledTime)```: Creates and starts a YCoroutine that waits specified time in seconds, using either scaled or unscaled time.
5. ```StopAndNull(ref YCoroutine cor)```: Safely stops a YCoroutine and nullifies its reference, helping to prevent memory leaks and dangling references.

## Future features (not implemented yet)
1. Debug editor for all active coroutines
2. C# Tasks support
