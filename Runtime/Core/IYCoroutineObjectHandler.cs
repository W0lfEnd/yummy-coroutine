namespace YummyCoroutine.Runtime.Core
{
    public interface IYCoroutineObjectHandler
    {
        public bool CanHandle(object obj);
        public void Handle(IYCoroutine coroutine, object obj);
        public bool CanReturnYieldValue { get; }
        public object NewYieldValue { get; }
    }
}