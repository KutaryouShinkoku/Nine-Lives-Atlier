using System;
using UnityEngine;
using TMPro;
using UnityEngine.Pool;
using WanFramework.UI;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.UI.InGame
{
    public class UIPopupNumberView : UIRootView
    {
        [Header("Damage Settings")]
        [SerializeField] private float damageDuration = 1.2f;
        [SerializeField] private Vector2 damageOffsetRange = new(-0.5f, 0.5f);
        [SerializeField] private float floatHeight = 2f;
        [SerializeField] private Color damageColor = Color.red;

        [Header("Text Settings")]
        [SerializeField] private AnimationCurve fontSizeCurve = AnimationCurve.Linear(0, 20, 1, 40);
        [SerializeField] private float maxFontSize = 60f;
        [SerializeField] private float minFontSize = 20f;
        [SerializeField] private float maxReferenceValue = 100f;
        [SerializeField] private float angleRange = 15f;

        [Header("Buff Settings")]
        [SerializeField] private float buffDuration = 1.5f;
        [SerializeField] private Vector3 buffOffset = new(0, 0.5f, 0);

        [Header("Prefabs")]
        [SerializeField] private TMP_Text damageTextPrefab;
        [SerializeField] private GameObject buffPopupPrefab;

        private ObjectPool<TMP_Text> damagePool;
        private ObjectPool<GameObject> buffPool;

        protected override void InitComponents()
        {
            base.InitComponents();
            InitializePools();
        }

        private void InitializePools()
        {
            damagePool = new ObjectPool<TMP_Text>(
                createFunc: () => CreatePooledItem(damageTextPrefab.gameObject).GetComponent<TMP_Text>(),
                actionOnRelease: text => text.gameObject.SetActive(false),
                defaultCapacity: 20
            );

            buffPool = new ObjectPool<GameObject>(
                createFunc: () => CreatePooledItem(buffPopupPrefab),
                actionOnRelease: go => go.SetActive(false),
                defaultCapacity: 10
            );
        }

        private GameObject CreatePooledItem(GameObject prefab)
        {
            var go = Instantiate(prefab, transform);
            go.transform.localScale = Vector3.one;
            go.SetActive(false);
            return go;
        }

        public async UniTask ProcessDamage(Vector3 worldPos, int amount, CancellationToken token)
        {
            var text = damagePool.Get();
            text.gameObject.SetActive(true);

            // 随机方向偏移
            var randomOffset = new Vector3(
                Random.Range(damageOffsetRange.x, damageOffsetRange.y),
                0,
                0
            );

            // 设置初始位置和角度
            //先往上加一个十五看看
            text.transform.position = worldPos + randomOffset + new Vector3(0, 15, 0);
            text.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-angleRange, angleRange));

            // 计算标准化数值（0-1范围）
            float normalizedValue = Mathf.Clamp01(Mathf.Abs(amount) / maxReferenceValue);

            // 根据曲线计算基础字号
            float baseSize = fontSizeCurve.Evaluate(normalizedValue);

            // 限制字号范围
            float fontSize = Mathf.Clamp(baseSize, minFontSize, maxFontSize);
            text.fontSize = fontSize;

            text.color = damageColor;
            text.text = amount.ToString("+#;-#;0");

            var startPos = text.transform.position;
            var endPos = startPos + Vector3.up * floatHeight;
            var canvasGroup = text.GetComponent<CanvasGroup>() ?? text.gameObject.AddComponent<CanvasGroup>();

            // 修改动画协程
            try
            {
                await AnimatePopup(canvasGroup, text, startPos, endPos, damageDuration, token);
            }
            finally
            {
                damagePool.Release(text);
            }
        }

        private async UniTask AnimatePopup(CanvasGroup group, TMP_Text text, Vector3 startPos, Vector3 endPos, float duration, CancellationToken token)
        {
            float elapsed = 0;
            float originalFontSize = text.fontSize;
            Vector3 originalScale = text.transform.localScale;

            // 添加随机运动曲线参数
            float xOffset = Random.Range(-0.3f, 0.3f);
            float yOffset = Random.Range(0.8f, 1.2f);

            while (elapsed < duration)
            {
                float progress = elapsed / duration;

                // 抛物线运动
                Vector3 newPos = Vector3.Lerp(startPos, endPos, progress);
                newPos.x += Mathf.Sin(progress * Mathf.PI) * xOffset;
                newPos.y += Mathf.Sin(progress * Mathf.PI) * yOffset;

                // 字体大小动态变化
                float sizeFactor = Mathf.Lerp(1f, 0.8f, progress);
                text.transform.localScale = originalScale * sizeFactor;

                // 透明度变化
                group.alpha = Mathf.Lerp(1, 0, progress);

                // 轻微旋转动画
                text.transform.Rotate(0, 0, 45 * Time.deltaTime * (Random.value > 0.5f ? 1 : -1));

                group.transform.position = newPos;
                elapsed += Time.deltaTime;
                await UniTask.NextFrame(token);
            }

            // 重置状态
            text.transform.localScale = originalScale;
            text.transform.rotation = Quaternion.identity;
        }

        public async UniTask ProcessBuff(Vector3 worldPos, Sprite icon, CancellationToken token)
        {
            var buffGo = buffPool.Get();
            buffGo.SetActive(true);

            buffGo.transform.position = worldPos + buffOffset;
            var image = buffGo.GetComponent<Image>();
            image.sprite = icon;

            var canvasGroup = buffGo.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;

            try
            {
                await AnimatePopup(canvasGroup,
                    buffGo.transform.position,
                    buffGo.transform.position + Vector3.up,
                    buffDuration,
                    token);
            }
            finally
            {
                buffPool.Release(buffGo);
            }
        }

        private async UniTask AnimatePopup(CanvasGroup group, Vector3 startPos, Vector3 endPos, float duration, CancellationToken token)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                group.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                group.alpha = Mathf.Lerp(1, 0, elapsed / duration);
                elapsed += Time.deltaTime;
                await UniTask.NextFrame(token);
            }
        }
    }
}