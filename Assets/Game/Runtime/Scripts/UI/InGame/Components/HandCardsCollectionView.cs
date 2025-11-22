using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Model.InGameSubModel;
using Game.UI.Common;
using Game.UI.Common.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.Utils;

namespace Game.UI.InGame.Components
{
    public partial class HandCardsCollectionView : CollectionView<CardModel>
    {
        private struct LayoutInfo
        {
            public CommonCardView View;
            public Vector3 TargetPosition;
            public Vector3 Velocity;
            public Quaternion TargetRotation;
            public Quaternion RotationDeriv;
            public bool AutoScale;
            public float ElementWidth;
            public float SmoothTime;
            public float RotSmoothTime;
            public bool IgnoreCurve;
            public Vector3 TargetPositionAnimOffset;
            public bool IgnoreLayout;
            public bool IgnoreAnim;
            public bool IgnoreAll;
        }
        
        // 定位点，驱动卡牌移动动效
        private readonly List<LayoutInfo> _layoutInfos = new();
        
        [Header("Curve Parameters")]
        [SerializeField]
        private CurveParameters curveParameters;

        [Header("Layout Parameters")]
        [SerializeField]
        private float cardWidth = 31.5f;
        [SerializeField]
        private float draggingCardWidth = 45.0f;
        [SerializeField]
        private float smoothTime = 0.2f;
        [SerializeField]
        private float draggingSmoothTime = 0.01f;
        [SerializeField]
        private float rotSmoothTime = 0.2f;
        [SerializeField]
        private float draggingRotSmoothTime = 0.1f;
        [SerializeField]
        private float maxTotalWidth = 250f;
        
        // 事件
        public UnityEvent<int, int> onCardDown;
        public UnityEvent<int> onCardUp;
        public UnityEvent<int> onCardEnter;
        public UnityEvent<int> onCardExit;
        public UnityEvent<int> onCardDestroy;
        public UnityEvent<int, int> onCardClick;
        
        private UnityAction<CommonCardView, int> _onCardDownListener;
        private UnityAction<CommonCardView> _onCardUpListener;
        private UnityAction<CommonCardView> _onCardEnterListener;
        private UnityAction<CommonCardView> _onCardExitListener;
        private UnityAction<CommonCardView, int> _onCardClickListener;

        // 鼠标进入卡牌Id
        private int _selectedCardIndex = -1;
        // 拖拽相关字段
        private int _draggedCardIndex = -1;
        private bool _isDragging = false;
        private Vector3 _dragOffset = Vector3.zero;

        private bool _interactable;
        public bool Interactable
        {
            get => _interactable;
            set
            {
                for (var i = 0; i < _layoutInfos.Count; i++)
                    _layoutInfos[i].View.Interactable = value;
                _interactable = value;
            }
        }
        protected override void InitComponents()
        {
            base.InitComponents();
            _onCardDownListener = OnCardDown;
            _onCardUpListener = OnCardUp;
            _onCardEnterListener = OnCardEnter;
            _onCardExitListener = OnCardExit;
            _onCardClickListener = OnCardClick;
        }
        
        protected override void OnElementAdding(SubView subView, CardModel newElement, int newIndex)
        {
            base.OnElementAdding(subView, newElement, newIndex);
            _layoutInfos.Insert(newIndex, new LayoutInfo
            {
                View = subView as CommonCardView,
                AutoScale = true,
                ElementWidth = cardWidth,
                SmoothTime = smoothTime,
                RotSmoothTime = rotSmoothTime,
            });
            subView.DataModel = newElement;
            var cardView = (CommonCardView)subView;
            cardView.onCardDown.AddListener(_onCardDownListener);
            cardView.onCardUp.AddListener(_onCardUpListener);
            cardView.onCardEnter.AddListener(_onCardEnterListener);
            cardView.onCardExit.AddListener(_onCardExitListener);
            cardView.onCardClick.AddListener(_onCardClickListener);
            cardView.Interactable = _interactable;
            cardView.IsVisible = true;
            RecalculateLayoutPoint();
            MoveToTargetPosition(newIndex);
        }

        protected override void OnElementRemoving(SubView subView, int oldIndex)
        {
            base.OnElementRemoving(subView, oldIndex);
            OnCardDestroy(oldIndex);
            _layoutInfos.RemoveAt(oldIndex);
            subView.DataModel = null;
            var cardView = (CommonCardView)subView;
            cardView.onCardDown.RemoveListener(_onCardDownListener);
            cardView.onCardUp.RemoveListener(_onCardUpListener);
            cardView.onCardEnter.RemoveListener(_onCardEnterListener);
            cardView.onCardExit.RemoveListener(_onCardExitListener);
            cardView.onCardClick.RemoveListener(_onCardClickListener);
        }
        
        private void MoveToTargetPosition(int cardIndex)
            => GetSubView(cardIndex).transform.localPosition = _layoutInfos[cardIndex].TargetPosition;

        private void RecalculateLayoutPoint()
        {
            RecalculateBaseLayoutPoint();
            ApplyDraggingLayoutPoint();
            ApplyCurveOffset();
        }
        private void ApplyDraggingLayoutPoint()
        {
            if (_isDragging && _draggedCardIndex >= 0 && _draggedCardIndex < _layoutInfos.Count)
            {
                SubView draggedView = GetSubView(_draggedCardIndex);
                float fixedWorldZ = draggedView.transform.position.z;
                float distanceFromCamera = fixedWorldZ - UISystem.Instance.UICamera.transform.position.z;

                // 获取当前鼠标屏幕坐标，并设置 z 值为distanceFromCamera
                Vector3 mouseScreenPos = Pointer.current.position.ReadValue();
                mouseScreenPos.z = distanceFromCamera;

                Vector3 mouseWorldPos = UISystem.Instance.UICamera.ScreenToWorldPoint(mouseScreenPos);
                Vector3 mouseLocalPos = transform.InverseTransformPoint(mouseWorldPos);

                // 计算新的局部坐标
                Vector3 newLocalPos = mouseLocalPos + _dragOffset;
                newLocalPos.z = draggedView.transform.localPosition.z;
                
                // 更新目标坐标
                _layoutInfos.GetRef(_draggedCardIndex).TargetPosition = newLocalPos;
            }
        }
        
        private void RecalculateBaseLayoutPoint()
        {
            var layoutInfoCount = _layoutInfos.Count;
            if (layoutInfoCount == 0) return;
            var totalWidth = 0.0f;
            var canScaleWidth = 0.0f;
            for (var i = 0; i < layoutInfoCount; i++)
                if (_layoutInfos[i].View.IsVisible && !_layoutInfos[i].IgnoreLayout)
                {
                    totalWidth += _layoutInfos[i].ElementWidth;
                    if (_layoutInfos[i].AutoScale) canScaleWidth += _layoutInfos[i].ElementWidth;
                }
            var fixedWidth = totalWidth - canScaleWidth;
            var widthScale = 1.0f;
            if (totalWidth != 0) widthScale = Mathf.Min(1.0f, (maxTotalWidth - fixedWidth) / canScaleWidth);
            totalWidth = fixedWidth + canScaleWidth * widthScale;
            var curPos = new Vector3(-totalWidth * 0.5f, 0, 0);
            var prevIndex = -1;
            for (var i = 0; i < layoutInfoCount; i++)
            {
                if (!_layoutInfos[i].View.IsVisible || _layoutInfos[i].IgnoreLayout) continue;
                if (prevIndex != -1)
                    curPos += new Vector3(_layoutInfos[prevIndex].ElementWidth / 2, 0, 0) * 
                              (_layoutInfos[prevIndex].AutoScale ? widthScale : 1);
                prevIndex = i;
                curPos += new Vector3(_layoutInfos[i].ElementWidth / 2, 0, 0) * 
                          (_layoutInfos[i].AutoScale ? widthScale : 1);
                _layoutInfos.GetRef(i).TargetPosition = curPos ;
            }
        }

        public void SetIgnoreLayout(int cardIndex, bool ignore)
        {
            _layoutInfos.GetRef(cardIndex).IgnoreLayout = ignore;
            if (_draggedCardIndex == cardIndex)
                StopDragCard();
        }
        
        public void Update()
        {
            RecalculateLayoutPoint();
            UpdateLayoutAnim();
            for (var i = 0; i < _layoutInfos.Count; i++)
                _layoutInfos.GetRef(i).TargetRotation = Quaternion.identity;
            ApplyRotationByVelocity();
            ApplyRotationByCurve();
            UpdateRotation();
        }

        private void UpdateLayoutAnim()
        {
            for (int i = 0; i < _layoutInfos.Count; i++)
            {
                if (!_layoutInfos[i].View.IsVisible || _layoutInfos[i].IgnoreAll) continue;
                SubView view = _layoutInfos[i].View;
                Vector3 targetPosition = _layoutInfos[i].TargetPosition + _layoutInfos[i].TargetPositionAnimOffset;
                Vector3 currentPosition = view.transform.localPosition;
                Vector3 velocity = _layoutInfos[i].Velocity;

                Vector3 newPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, _layoutInfos[i].SmoothTime);
                view.transform.localPosition = newPosition;
                ref var layoutRef = ref _layoutInfos.GetRef(i);
                layoutRef.Velocity = velocity;
                layoutRef.TargetPositionAnimOffset = Vector3.zero;
            }
        }

        private void ApplyRotationByVelocity()
        {
            // TODO 之后可能需要拆到MonoBehaviour里
            for (int i = 0; i < GetSubViewCount(); i++)
            {
                var velocity = _layoutInfos[i].Velocity;
                var rotScale = 0.1f;
                _layoutInfos.GetRef(i).TargetRotation *= Quaternion.Euler(
                    Mathf.Clamp(velocity.y * rotScale, -15, 15), 
                    Mathf.Clamp(velocity.z * rotScale, -15, 15), 
                    Mathf.Clamp(-velocity.x * rotScale, -15, 15));
            }
        }

        private void ApplyRotationByCurve()
        {
            for (int i = 0; i < GetSubViewCount(); i++)
            {
                if (_layoutInfos[i].IgnoreCurve || _layoutInfos[i].IgnoreAll) continue;
                const float dx = 0.05f;
                var normalizedPosition = GetNormalizedPosition(i) * (1 - dx) + dx / 2;
                var l = curveParameters.positioning.Evaluate(normalizedPosition - dx / 2) * curveParameters.positioningInfluence;
                var r = curveParameters.positioning.Evaluate(normalizedPosition + dx / 2) * curveParameters.positioningInfluence;
                var delta = (r - l) / dx;
                _layoutInfos.GetRef(i).TargetRotation *= Quaternion.Euler(0, 0, delta * 0.25f);
            }
        }

        private void UpdateRotation()
        {
            for (var i = 0; i < GetSubViewCount(); i++)
            {
                if (!_layoutInfos[i].View.IsVisible) continue;
                var view = GetSubView(i);
                view.transform.localRotation = QuaternionUtil.SmoothDamp(view.transform.localRotation, _layoutInfos[i].TargetRotation,
                    ref _layoutInfos.GetRef(i).RotationDeriv, _layoutInfos[i].RotSmoothTime);
            }
        }
        /// <summary>
        /// 应用曲线偏移，根据CurveParameters调整卡牌的位置
        /// </summary>
        private void ApplyCurveOffset()
        {
            if (curveParameters == null)
            {
                Debug.LogWarning("CurveParameters is not assigned.");
                return;
            }

            // 在栈上创建一个新的Span来存储应用偏移后的布局点
            Span<Vector3> offsetLayoutPoints = stackalloc Vector3[_layoutInfos.Count];
            for (int i = 0; i < _layoutInfos.Count; i++)
            {
                if (_layoutInfos[i].IgnoreCurve || _layoutInfos[i].IgnoreLayout)
                {
                    offsetLayoutPoints[i] = _layoutInfos[i].TargetPosition;
                    continue;
                }
                var normalizedPosition = GetNormalizedPosition(i);
                var curveYOffset = curveParameters.positioning.Evaluate(normalizedPosition) * curveParameters.positioningInfluence;
                var offsetPos = _layoutInfos[i].TargetPosition + new Vector3(0, curveYOffset, 0);
                offsetLayoutPoints[i] = offsetPos;
            }

            // 更新布局点为带偏移的布局点
            for (int i = 0; i < _layoutInfos.Count; i++)
            {
                _layoutInfos.GetRef(i).TargetPosition = offsetLayoutPoints[i];
            }
        }


        /// <summary>
        /// 获取卡牌的归一化位置
        /// </summary>
        /// <param name="index">卡牌索引</param>
        /// <returns>0到1之间的值，表示卡牌在手牌中的位置</returns>
        private float GetNormalizedPosition(int index)
        {
            var layoutSubViewCount = 0;
            var newIndex = 0;
            for (var i = 0; i < _layoutInfos.Count; i++)
            {
                if (!_layoutInfos[i].View.IsVisible ||
                    _layoutInfos[i].IgnoreCurve ||
                    _layoutInfos[i].IgnoreAll ||
                    _layoutInfos[i].IgnoreLayout) continue;
                ++layoutSubViewCount;
                if (i < index) ++newIndex;
            }
            if (layoutSubViewCount <= 1)
                return 0.5f;

            return Mathf.Clamp01((float)newIndex / (layoutSubViewCount - 1));
        }

        public void StartDragCard(int index, bool keepState = false)
        {
            if (index < 0 || index >= GetSubViewCount())
                return;
            BeginSelectCard(index);
            var oldLayoutInfo = new LayoutInfo();
            if (_draggedCardIndex != -1 && _draggedCardIndex < GetSubViewCount())
            {
                oldLayoutInfo = _layoutInfos[_draggedCardIndex];
                StopDragCard();
            }

            _draggedCardIndex = index;
            _isDragging = true;

            // 获取被拖拽卡牌的世界z值
            var draggedView = (GetSubView(_draggedCardIndex) as CommonCardView)!;
            float fixedWorldZ = draggedView.transform.position.z;

            // 计算从摄像机到卡牌所在平面的距离
            float distanceFromCamera = fixedWorldZ - UISystem.Instance.UICamera.transform.position.z;

            Vector3 mouseScreenPos = Pointer.current.position.ReadValue();
            mouseScreenPos.z = distanceFromCamera;

            // 转换坐标
            if (!keepState)
            {
                var mouseWorldPos = UISystem.Instance.UICamera.ScreenToWorldPoint(mouseScreenPos);
                var mouseLocalPos = transform.InverseTransformPoint(mouseWorldPos);
                _dragOffset = draggedView.transform.localPosition - mouseLocalPos;
            }
            
            // 拖拽模式忽略Curve
            ref var layoutInfo = ref _layoutInfos.GetRef(_draggedCardIndex);
            layoutInfo.IgnoreCurve = true;
            layoutInfo.IgnoreAnim = true;
            layoutInfo.SmoothTime = draggingSmoothTime;
            layoutInfo.RotSmoothTime = draggingRotSmoothTime;
            if (keepState)
            {
                layoutInfo.Velocity = oldLayoutInfo.Velocity;
                layoutInfo.TargetPosition = oldLayoutInfo.TargetPosition;
                layoutInfo.TargetRotation = oldLayoutInfo.TargetRotation;
            }
        }
        public void StopDragCard()
        {
            _isDragging = false;
            if (_draggedCardIndex == -1) return;
            // StopSelectCard();
            // 拖拽模式结束，启用Curve
            ref var layoutInfo = ref _layoutInfos.GetRef(_draggedCardIndex);
            layoutInfo.IgnoreCurve = false;
            layoutInfo.IgnoreAnim = false;
            layoutInfo.SmoothTime = smoothTime;
            layoutInfo.RotSmoothTime = rotSmoothTime;
            _draggedCardIndex = -1;
        }

        public void BeginSelectCard(int index)
        {
            if (index < 0 || index >= GetSubViewCount())  return;
            if (_selectedCardIndex == index) return;
            if (_selectedCardIndex != -1) StopSelectCard();
            _selectedCardIndex = index;
            // 开始选择卡牌
            var draggedView = (GetSubView(_selectedCardIndex) as CommonCardView)!;
            // 设置选择牌的布局属性
            ref var layoutInfo = ref _layoutInfos.GetRef(_selectedCardIndex);
            layoutInfo.ElementWidth = draggingCardWidth;
            layoutInfo.AutoScale = false;
        }

        public void StopSelectCard()
        {
            if (_selectedCardIndex == -1) return;
            if (_selectedCardIndex >= _layoutInfos.Count)
            {
                _selectedCardIndex = -1;
                return;
            }
            // 重置选择牌的布局属性
            ref var layoutInfo = ref _layoutInfos.GetRef(_selectedCardIndex);
            layoutInfo.ElementWidth = cardWidth;
            _selectedCardIndex = -1;
            layoutInfo.AutoScale = true;
        }
        public void SetTargetPosition(int index, Vector3 targetPosition)
        {
            ref var layoutInfo = ref _layoutInfos.GetRef(index);
            layoutInfo.TargetPosition = transform.InverseTransformPoint(targetPosition);
        }
        private int GetIndexForSubView(SubView view)
        {
            for (var i = 0; i < GetSubViewCount(); i++)
                if (GetSubView(i) == view)
                    return i;
            return -1;
        }
        private void OnCardDown(CommonCardView cardView, int btn) => onCardDown?.Invoke(GetIndexForSubView(cardView), btn);
        private void OnCardUp(CommonCardView cardView) => onCardUp?.Invoke(GetIndexForSubView(cardView));
        private void OnCardEnter(CommonCardView cardView) => onCardEnter?.Invoke(GetIndexForSubView(cardView));
        private void OnCardExit(CommonCardView cardView) => onCardExit?.Invoke(GetIndexForSubView(cardView));
        private void OnCardDestroy(int cardViewIndex) => onCardDestroy?.Invoke(cardViewIndex);
        private void OnCardClick(CommonCardView cardView, int button) => onCardClick?.Invoke(GetIndexForSubView(cardView), button);
    }
}