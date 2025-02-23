using System.Collections.Generic;

namespace YummyCoroutine.Runtime.Core
{
    public partial class YCoroutine
    {
        protected struct CustomHandlerResult
        {
            public bool IsHandled;
            public bool NeedYieldReturn;
            public object YieldValue;
        }

        private static readonly List<IYCoroutineObjectHandler> _customHandlers = new();

        public static void AddCustomHandler(IYCoroutineObjectHandler handler)
        {
            _customHandlers.Add(handler);
        }

        public static void RemoveCustomHandler(IYCoroutineObjectHandler handler)
        {
            _customHandlers.Remove(handler);
        }

        public static void ClearCustomHandlers()
        {
            _customHandlers.Clear();
        }

        private CustomHandlerResult TryCustomHandle(object yieldValue)
        {
            CustomHandlerResult result = default;
            foreach (var handler in _customHandlers)
            {
                if (handler.CanHandle(yieldValue))
                {
                    handler.Handle(this, yieldValue);
                    result.IsHandled = true;
                    result.NeedYieldReturn = handler.CanReturnYieldValue;
                    result.YieldValue = handler.NewYieldValue;
                    break;
                }
            }

            return result;
        }
    }
}