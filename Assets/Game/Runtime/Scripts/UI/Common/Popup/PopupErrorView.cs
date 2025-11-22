using UnityEngine;
using WanFramework.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using Game.Audio;
using Game.Data;

public enum ErrorTipType
{ 
    HandFull,
    InsufficientElements,
    ShopNotEnoughMoney,
    ShopNoValSelected
}

[Serializable]
public class ErrorTipMapping
{
    public ErrorTipType type;
    public GameObject textObject;
}

namespace Game.UI.Common
{
    public class PopupErrorView : UIRootView
    {

        [SerializeField] 
        private float _visibleDuration = 2f;  // 持续显示时间
        [SerializeField] 
        private float _fadeDuration = 0.5f;  // 淡出持续时间

        // 存储不同错误类型对应的文本对象
        [SerializeField]
        private List<ErrorTipMapping> errorMappings = new();

        private CanvasGroup _canvasGroup;
        private Coroutine _currentFadeCoroutine;
        private Dictionary<ErrorTipType, GameObject> _errorTextMap;

        public void SetError(ErrorTipType type)
        {
            // 隐藏所有文本
            foreach (var textObj in _errorTextMap.Values)
            {
                textObj.SetActive(false);
            }

            // 显示目标文本
            if (_errorTextMap.TryGetValue(type, out var targetText))
            {
                targetText.SetActive(true);
            }
            else
            {
                Debug.LogError($"No text mapping found for error type: {type}");
            }
        }

        protected override void InitComponents()
        {
            base.InitComponents();

            // 初始化字典
            _errorTextMap = new();
            foreach (var mapping in errorMappings)
            {
                _errorTextMap[mapping.type] = mapping.textObject;
                mapping.textObject.SetActive(false); // 默认隐藏
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _canvasGroup.alpha = 1f;
        }

        public override void OnShow()
        {
            base.OnShow();

            if (_currentFadeCoroutine != null)
                StopCoroutine(_currentFadeCoroutine);

            // 立即显示
            _canvasGroup.alpha = 1f;

            //播放音效
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Error);

            // 启动自动消失协程
            _currentFadeCoroutine = StartCoroutine(AutoHideRoutine());
        }

        private IEnumerator AutoHideRoutine()
        {
            // 等待持续显示时间
            yield return new WaitForSeconds(_visibleDuration);

            // 执行淡出动画
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _canvasGroup.alpha = 0f;

            UISystem.Instance.Hide(this);
        }

        public override void OnHide()
        {
            if (_currentFadeCoroutine != null)
                StopCoroutine(_currentFadeCoroutine);

            base.OnHide();
        }
    }
}