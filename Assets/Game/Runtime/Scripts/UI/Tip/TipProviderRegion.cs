using System;
using System.Collections.Generic;
using Game.Data;
using Game.Localization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using WanFramework.Base;

namespace Game.UI.Tip
{
    public enum TipDirectionType
    {
        Up,
        Down
    }
    public enum TipProviderRegionType
    {
        MouseEnter,
        MouseHoverNotPressed,
        MouseDown
    }
    public enum TipProviderPositionType
    {
        LeftOrRight,
        FollowMouse,
        FollowProvider
    }
    [Serializable]
    public struct TipPositionOffset
    {
        public Vector3 left;
        public Vector3 right;
    }
    /// <summary>
    /// 提供Tip的可交互区域
    /// </summary>
    public class TipProviderRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [SerializeField]
        private List<TipBoxData> tips;
        [SerializeField]
        private TipProviderRegionType regionType;
        [SerializeField]
        private TipProviderPositionType positionType;
        [SerializeField]
        private TipDirectionType directionType;
        [SerializeField]
        private bool reverseOrder = false;
        
        [SerializeField]
        [Tooltip("当positionType为FollowProvider时启用")]
        private TipPositionOffset positionOffset;
        
        private bool _isPressed = false;
        private bool _isEnter = false;
        
        public TipProviderPositionType GetPositionType() => positionType;
        public TipDirectionType GetDirectionType() => directionType;
        public bool GetIsReverse() => reverseOrder;
        public TipPositionOffset GetPositionOffset() => positionOffset;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isEnter = true;
            if (regionType == TipProviderRegionType.MouseEnter && eventData.pointerPress == null) TipUtils.ActivateTipProvider(this);
            if (regionType == TipProviderRegionType.MouseHoverNotPressed && !Pointer.current.press.isPressed) TipUtils.ActivateTipProvider(this);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            _isEnter = false;
            if (regionType == TipProviderRegionType.MouseEnter) TipUtils.DeactivateTipProvider(this);
            if (regionType == TipProviderRegionType.MouseHoverNotPressed) TipUtils.DeactivateTipProvider(this);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            if (regionType == TipProviderRegionType.MouseDown) TipUtils.ActivateTipProvider(this);
            if (InputHelper.GetPointerType() != PointerType.Mouse)
            {
                if (regionType == TipProviderRegionType.MouseHoverNotPressed) TipUtils.ActivateTipProvider(this);
            }
            else
            {
                if (regionType == TipProviderRegionType.MouseHoverNotPressed) TipUtils.DeactivateTipProvider(this);
            }
        }
        public void OnPointerMove(PointerEventData eventData)
        {
            if (InputHelper.GetPointerType() != PointerType.Mouse)
                if (regionType == TipProviderRegionType.MouseHoverNotPressed && _isPressed) TipUtils.DeactivateTipProvider(this);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            if (regionType == TipProviderRegionType.MouseDown) TipUtils.DeactivateTipProvider(this);
            if (InputHelper.GetPointerType() != PointerType.Mouse)
                if (regionType == TipProviderRegionType.MouseHoverNotPressed) TipUtils.DeactivateTipProvider(this);
        }
        private void OnDisable()
        {
            TipUtils.DeactivateTipProvider(this);
            _isEnter = false;
            _isPressed = false;
        }
        public IEnumerable<TipBoxData> GetTips() => tips;
        public void SetTips(IEnumerable<TipBoxData> newTips)
        {
            tips.Clear();
            tips.AddRange(newTips);
        }
        public void AddTip(TipBoxData newTips)
        {
            if (tips.Contains(newTips)) return;
            tips.Add(newTips);
            TipUtils.GetTipsAndAddToRegion(newTips.Title.Local(), this);
            TipUtils.GetTipsAndAddToRegion(newTips.Content.Local(), this);
        }
        public void ClearTip() => tips.Clear();
        
        public void OnDrawGizmosSelected()
        {
            if (positionType != TipProviderPositionType.FollowProvider) return;
            Gizmos.color = Color.green;
            var scaledLeft = Vector3.Scale(positionOffset.left, transform.lossyScale);
            var scaledRight = Vector3.Scale(positionOffset.right, transform.lossyScale);
            Gizmos.DrawLine(transform.position + scaledLeft, transform.position + scaledLeft + Vector3.up * 1000);
            Gizmos.DrawLine(transform.position + scaledRight, transform.position + scaledRight + Vector3.up * 1000);
        }
    }
}