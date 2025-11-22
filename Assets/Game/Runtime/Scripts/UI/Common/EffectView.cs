using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using WanFramework.Resource;
using WanFramework.UI;

namespace Game.UI.Common
{
    public class EffectView : RootView
    {
        private Dictionary<string, ObjectPool<GameObject>> _effectPools = new();
        private static EffectView _instance;
        private async UniTask<GameObject> CreateEffectAsync(string effect, CancellationToken token)
        {
            if (!_effectPools.TryGetValue(effect, out var effectPool))
            {
                var prefab = await ResourceSystem.Instance.LoadAsyncUniTask<GameObject>(effect, token);
                effectPool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab, transform, true)
                    );
                _effectPools[effect] = effectPool;
            }
            var obj = effectPool.Get();
            obj.SetActive(true);
            return obj;
        }
        private void ReleaseEffect(string effect, GameObject go)
        {
            if (!_effectPools.TryGetValue(effect, out var effectPool)) return;
            go.gameObject.SetActive(false);
            effectPool.Release(go);
        }
        private async UniTask ShowEffectInner(string effect, float time, Vector3 posMin, Vector3 posMax, CancellationToken token)
        {
            var posX = Random.Range(posMin.x, posMax.x);
            var posY = Random.Range(posMin.x, posMax.x);
            var posZ = Random.Range(posMin.x, posMax.x);
            var pos = new Vector3(posX, posY, posZ);
            effect = $"Content/{effect}.prefab";
            var loadingBeginTime = Time.time;
            var effectObject = await CreateEffectAsync(effect, token);
            var loadingTime = Time.time - loadingBeginTime;
            effectObject.transform.position = pos;
            try
            {
                if (time <= 0)
                    await UniTask.Never(token);
                else if (time - loadingTime > 0)
                    await UniTask.WaitForSeconds(time - loadingTime, cancellationToken: token, cancelImmediately: true);
            }
            finally
            {
                ReleaseEffect(effect, effectObject);
            }
        }
        public static UniTask ShowEffect(string effect, float time, Vector3 posMin, Vector3 posMax, CancellationToken cts)
        {
            if (_instance == null) _instance = UISystem.Instance.ShowCommonView<EffectView>("Common/Effect");
            return _instance.ShowEffectInner(effect, time, posMin, posMax, cts);
        }
        public static UniTask ShowEffect(string effect, Vector3 posMin, Vector3 posMax, CancellationToken cts) =>
            ShowEffect(effect, 0, posMin, posMax, cts);
        public static UniTask ShowEffect(string effect, float time, Vector3 pos, CancellationToken cts) =>
            ShowEffect(effect, time, pos, pos, cts);
        public static UniTask ShowEffect(string effect, Vector3 pos, CancellationToken cts) =>
            ShowEffect(effect, 0, pos, cts);
    }
}