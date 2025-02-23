using System.Linq;
using UnityEngine;

namespace YummyCoroutine.Runtime.Core
{
    public partial class YCoroutine
    {
        private class Coroutiner : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private static Coroutiner _coroutineCoroutiner = null;

        public static void Init()
        {
            if (_coroutineCoroutiner)
                return;

            _coroutineCoroutiner = new GameObject("YCoroutinesRoot").AddComponent<Coroutiner>();
        }

        public static void Deinit(bool destroyCoroutiner = false)
        {
            if (!_coroutineCoroutiner)
                return;

            Debug.Log($"{nameof(destroyCoroutiner)} {destroyCoroutiner}");
            while (_allActiveCoroutines.Count > 0)
            {
                YCoroutine cor = _allActiveCoroutines.Last();
                cor.Stop();
                _allActiveCoroutines.Remove(cor);
            }

            _coroutineCoroutiner.StopAllCoroutines();

            if (destroyCoroutiner)
                Object.Destroy(_coroutineCoroutiner.gameObject);
        }
    }
}