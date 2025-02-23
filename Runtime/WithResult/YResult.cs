namespace YummyCoroutine.Runtime.WithResult
{
    public class YResult<T> : IYResult
    {
        public T Result { get; private set; }

        object IYResult.Result
        {
            get => Result;
            set => Result = (T)value;
        }

        public YResult(T result)
        {
            Result = result;
        }
    }
}