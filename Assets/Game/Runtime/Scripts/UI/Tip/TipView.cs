using System;
using System.Buffers;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WanFramework.UI;

namespace Game.UI.Tip
{
    public class TipView : UIRootView
    {
        private readonly List<TipBoxComponent> _tipBoxes = new();
        private readonly HashSet<TipBoxData> _tips = new();
        private ObjectPool<TipBoxComponent> _boxPool;
        [SerializeField]
        private TipBoxComponent tipBoxTemplate;
        [SerializeField]
        private RectTransform transRoot;
        [SerializeField]
        private RectTransform anchorRoot;
        [SerializeField]
        private VerticalLayoutGroup layoutGroup;
        [SerializeField]
        private RectTransform leftSlot;
        [SerializeField]
        private RectTransform rightSlot;
        protected override void InitComponents()
        {
            base.InitComponents();
            _boxPool = new ObjectPool<TipBoxComponent>(() => Instantiate(tipBoxTemplate, tipBoxTemplate.transform.parent));
        }
        public void SetTips(IEnumerable<TipBoxData> tips)
        {
            ClearTip();
            foreach (var tip in tips) AddTip(tip);
        }
        public void AddTip(TipBoxData tipBoxData)
        {
            if (!_tips.Add(tipBoxData)) return;
            var tipBox = _boxPool.Get();
            tipBox.SetTip(tipBoxData);
            tipBox.gameObject.SetActive(true);
            _tipBoxes.Add(tipBox);
        }
        public void ClearTip()
        {
            foreach (var tipBox in _tipBoxes)
            {
                if (tipBox) tipBox.gameObject.SetActive(false);
                _boxPool.Release(tipBox);
            }
            _tipBoxes.Clear();
            _tips.Clear();
        }
        private void UpdatePosition()
        {
            if (!TipUtils.CurrentProviderRegion) return;
            var positionType = TipUtils.CurrentProviderRegion.GetPositionType();
            var directionType = TipUtils.CurrentProviderRegion.GetDirectionType();
            var orderType = TipUtils.CurrentProviderRegion.GetIsReverse();
            var mousePos = Pointer.current.position.ReadValue();
            anchorRoot.pivot = new Vector2(anchorRoot.pivot.x, directionType == TipDirectionType.Up ? 0 : 1);
            if (orderType)
                layoutGroup.reverseArrangement = directionType == TipDirectionType.Down;
            else
                layoutGroup.reverseArrangement = directionType == TipDirectionType.Up;
            if (positionType == TipProviderPositionType.FollowMouse)
            {
                // Follow mouse
                RectTransformUtility.ScreenPointToWorldPointInRectangle(transRoot, mousePos, UISystem.Instance.UICamera, out var worldPos);
                SetTipPos(worldPos);
            }
            else if (positionType == TipProviderPositionType.LeftOrRight)
            {
                // left or right
                transRoot.SetPositionAndRotation(TipUtils.TipActivePos.x > Screen.width / 2.0f ? leftSlot.position : rightSlot.position, Quaternion.identity);
            }
            else if (positionType == TipProviderPositionType.FollowProvider)
            {
                // Follow tip provider
                SetTipPos(TipUtils.CurrentProviderRegion.transform.position);
            }
        }
        private void Update() => UpdatePosition();
        protected override void OnEnable()
        {
            base.OnDisable();
            UpdatePosition();
        }
        private void SetTipPos(Vector3 position)
        {
            var region = TipUtils.CurrentProviderRegion;
            var screenPos = TipUtils.TipActivePos;
            var positionOffset = TipUtils.CurrentProviderRegion.GetPositionOffset();
            if (screenPos.x > Screen.width / 2.0f)
            {
                position += Vector3.Scale(positionOffset.left, region.transform.lossyScale);
                anchorRoot.pivot = new Vector2(1, anchorRoot.pivot.y);
                anchorRoot.anchoredPosition = Vector2.zero;
            }
            else
            {
                position += Vector3.Scale(positionOffset.right, region.transform.lossyScale);
                anchorRoot.pivot = new Vector2(0, anchorRoot.pivot.y);
                anchorRoot.anchoredPosition = Vector2.zero;
            }
            screenPos = UISystem.Instance.UICamera.WorldToScreenPoint(position);
            screenPos.z = UISystem.Instance.UICamera.WorldToScreenPoint(transform.position).z;
            transRoot.position = UISystem.Instance.UICamera.ScreenToWorldPoint(screenPos);
        }
    }
}