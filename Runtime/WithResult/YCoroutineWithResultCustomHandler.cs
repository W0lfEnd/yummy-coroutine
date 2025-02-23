using YummyCoroutine.Runtime.Core;

namespace YummyCoroutine.Runtime.WithResult
{
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
}