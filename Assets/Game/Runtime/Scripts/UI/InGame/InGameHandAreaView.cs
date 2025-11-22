using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.SM.InGameState;
using Game.SM.InGameState.BattleState;
using Game.UI.Common;
using Game.UI.Common.Components;
using Game.UI.InGame.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.InGame
{
    public class InGameHandAreaView : RootView
    {
        [SerializeField]
        private PendingUseCardsController pendingUseCardsController;
        
        [SerializeField]
        private RectTransform useDropArea;
        [SerializeField]
        private RectTransform sacrificeDropArea;
        
        [SerializeField]
        private Button buttonDrawCard;
        [SerializeField]
        private HandCardsCollectionView handCardsCollectionView;
        [SerializeField]
        private int _currentDraggingCardIndex = -1;
        private int _currentSelectedCardIndex = -1;
        
        private PlayerInteractionType _lastDraggingAction;
        
        private bool _canDragCard;
        private int _nextFrameDraggingCardIndex = -1;
        
        /// <summary>
        /// 是否启用手牌区的拖拽
        /// </summary>
        public bool CanDragCard
        {
            get => _canDragCard;
            set
            {
                if (!value && _canDragCard)
                    handCardsCollectionView.StopDragCard();
                _canDragCard = value;
                _currentDraggingCardIndex = -1;
            }
        }

        private bool _canSelectCard;
        /// <summary>
        /// 是否启用选牌
        /// </summary>
        public bool CanSelectCard
        {
            get => _canSelectCard;
            set
            {
                if (!value && _canSelectCard)
                    handCardsCollectionView.StopSelectCard();
                handCardsCollectionView.Interactable = value;
                _canSelectCard = value;
                _currentSelectedCardIndex = -1;
            }
        }
        
        private bool _canDrawCard;
        /// <summary>
        /// 是否启用抽牌
        /// </summary>
        public bool CanDrawCard
        {
            get => _canDrawCard;
            set
            {
                _canDrawCard = value;
                buttonDrawCard.gameObject.SetActive(_canDrawCard);
            }
        }
        
        public UnityEvent onDrawCard;
        public UnityEvent<int> onUseCard;
        public UnityEvent<int> onSacrificeCard;
        
        protected override void InitComponents()
        {
            base.InitComponents();
            handCardsCollectionView.onCardDown.AddListener(OnCardDown);
            handCardsCollectionView.onCardEnter.AddListener(OnCardEnter);
            handCardsCollectionView.onCardExit.AddListener(OnCardExit);
            handCardsCollectionView.onCardDestroy.AddListener(OnCardDestroy);
            handCardsCollectionView.onCardClick.AddListener(OnCardClick);
            buttonDrawCard.onClick.AddListener(OnDrawCard);
        }
        
        public CommonCardView GetCardView(int index) => handCardsCollectionView.GetSubView(index) as CommonCardView;
        private void Update()
        {
            if (Pointer.current.press.wasReleasedThisFrame)
                OnMouseUp();
            if (_nextFrameDraggingCardIndex != -1)
            {
                StartHighlightingCard(_nextFrameDraggingCardIndex);
                StartDraggingCard(_nextFrameDraggingCardIndex);
                _nextFrameDraggingCardIndex = -1;
            }
            if (Pointer.current.delta.ReadValue() == Vector2.zero) return;
            if (_currentDraggingCardIndex >= 0)
                OnCardDragging(_currentDraggingCardIndex);
            if (InputHelper.GetPointerType() != PointerType.Mouse)
            {
                if (_currentSelectedCardIndex >= 0 && _currentDraggingCardIndex < 0)
                    StartDraggingCard(_currentSelectedCardIndex);
            }
        }
        
        protected override void OnDataModelChanged(DataModelBase m)
        {
            if (m is BattleModel battleModel)
                handCardsCollectionView.ItemSource = battleModel.HandCards;
        }

        private void StartDraggingCard(int cardIndex, bool keepState = false)
        {
            if (_currentDraggingCardIndex != -1)
            {
                handCardsCollectionView.StopDragCard();
                GetCardView(_currentDraggingCardIndex).PlayUnselectAnim(CancellationToken.None).Forget();
            }
            handCardsCollectionView.StartDragCard(cardIndex, keepState);
            GetCardView(cardIndex).PlayDraggingAnim(CancellationToken.None).Forget();
            _currentDraggingCardIndex = cardIndex;
            _lastDraggingAction = PlayerInteractionType.None;
        }
        private void StartHighlightingCard(int cardIndex)
        {
            if (_currentDraggingCardIndex != -1) return;
            handCardsCollectionView.BeginSelectCard(cardIndex);
            GetCardView(cardIndex).PlaySelectAnim(CancellationToken.None).Forget();
            _currentSelectedCardIndex = cardIndex;
        }
        private void OnMouseUp()
        {
            if (!CanDragCard)
                return;
            if (_currentDraggingCardIndex != -1)
            {
                handCardsCollectionView.StopDragCard();
                var stillSelected = false;
                if (InputHelper.GetPointerType() == PointerType.Mouse)
                    stillSelected = GetCardView(_currentDraggingCardIndex).IsMouseOver;
                if (!stillSelected)
                {
                    GetCardView(_currentDraggingCardIndex).PlayUnselectAnim(CancellationToken.None).Forget();
                    _currentSelectedCardIndex = -1;
                    handCardsCollectionView.StopSelectCard();
                }
                OnCardDrop(_currentDraggingCardIndex);
            }
            _currentDraggingCardIndex = -1;
        }
        private void OnCardDown(int cardIndex, int btn)
        {
            if (!CanDragCard) return;
            if (btn != 0) return;
            if (InputHelper.GetPointerType() == PointerType.Mouse)
                StartDraggingCard(cardIndex);
            else
                StartHighlightingCard(cardIndex);
        }
        private void OnCardEnter(int cardIndex)
        {
            if (!CanSelectCard)
                return;
            StartHighlightingCard(cardIndex);
        }
        private void OnCardExit(int cardIndex)
        {
            if (_currentDraggingCardIndex != -1) return;
            if (_currentSelectedCardIndex != cardIndex) return;
            handCardsCollectionView.StopSelectCard();
            GetCardView(cardIndex).PlayUnselectAnim(CancellationToken.None).Forget();
            _currentSelectedCardIndex = -1;
        }
        /// <summary>
        /// 处理卡牌销毁后换交互牌的屎山
        /// </summary>
        /// <param name="cardIndex"></param>
        private void OnCardDestroy(int cardIndex)
        {
            if (_nextFrameDraggingCardIndex != -1)
            {
                if (_nextFrameDraggingCardIndex == cardIndex)
                    _nextFrameDraggingCardIndex = -1;
                else if (_nextFrameDraggingCardIndex > cardIndex)
                    _nextFrameDraggingCardIndex -= 1;
            }
            if (_currentDraggingCardIndex != -1)
            {
                if (_currentDraggingCardIndex == cardIndex)
                {
                    handCardsCollectionView.StopDragCard();
                    _currentDraggingCardIndex = -1;
                }
                else if (_currentSelectedCardIndex > cardIndex)
                {
                    _nextFrameDraggingCardIndex = _currentDraggingCardIndex - 1;
                    handCardsCollectionView.StopDragCard();
                    _currentDraggingCardIndex = -1;
                }
            }
            else
            {
                if (_currentSelectedCardIndex == cardIndex)
                {
                    handCardsCollectionView.StopSelectCard();
                    _currentSelectedCardIndex = -1;
                }
                else if (_currentSelectedCardIndex > cardIndex)
                {
                    handCardsCollectionView.StopSelectCard();
                    --_currentSelectedCardIndex;
                    handCardsCollectionView.BeginSelectCard(_currentSelectedCardIndex);
                }
            }
        }
        private void OnDrawCard()
        {
            // if (_currentSelectedCardIndex >= 0) return;
            onDrawCard?.Invoke();
        }
        private void OnCardClick(int cardIndex, int button)
        {
            if (!DataModel<SettingModel>.Instance.FastUse) return;
            if (InputHelper.GetPointerType() != PointerType.Mouse) return;
            if (button == 0) onUseCard?.Invoke(cardIndex);
            else if (button == 1) onSacrificeCard?.Invoke(cardIndex);
        }
        private PlayerInteractionType GetDropAction()
        {
            var screenPoint = Pointer.current.position.ReadValue();
            if (RectTransformUtility.RectangleContainsScreenPoint(sacrificeDropArea, screenPoint))
                return PlayerInteractionType.SacrificeCard;
            if (RectTransformUtility.RectangleContainsScreenPoint(useDropArea, screenPoint))
                return PlayerInteractionType.UseCard;
            return PlayerInteractionType.None;
        }
        private void OnCardDrop(int cardIndex)
        {
            var action = GetDropAction();
            if (action == PlayerInteractionType.UseCard) onUseCard?.Invoke(cardIndex);
            else if (action == PlayerInteractionType.SacrificeCard) onSacrificeCard?.Invoke(cardIndex);
        }
        private void OnCardDragging(int cardIndex)
        {
            var action = GetDropAction();
            if (_lastDraggingAction == action) return;
            if (action == PlayerInteractionType.UseCard) GetCardView(cardIndex).PlayDraggingUseAnim(CancellationToken.None).Forget();
            else if (action == PlayerInteractionType.SacrificeCard) GetCardView(cardIndex).PlayDraggingSacrificeAnim(CancellationToken.None).Forget();
            else GetCardView(cardIndex).PlayDraggingAnim(CancellationToken.None).Forget();
            _lastDraggingAction = action;
        }
        public void SetTargetPosition(int cardIndex, Vector3 position) => handCardsCollectionView.SetTargetPosition(cardIndex, position);
        public void SetIgnoreLayout(int cardIndex, bool ignore) => handCardsCollectionView.SetIgnoreLayout(cardIndex, ignore);
        public void SetCardInteractable(int cardIndex, bool interactable) => GetCardView(cardIndex).Interactable = interactable;

        public UniTask PlayPlayerAttackEnemyAnim(CancellationToken token)
        {
            return handCardsCollectionView.PlayWaveShakeCardsAnim(token, 0.1f, 15, 0.3f, 10, true, 0.5f);
        }
        public UniTask PlayEnemyAttackPlayerAnim(CancellationToken token)
        {
            return handCardsCollectionView.PlayExplosionAnim(token, new Vector3(0, 10, 0), 0.1f, 2, 200);
        }
        public void PendingUseCard(int index, PlayerInteractionType interactionType)
        {
            ((CommonCardView)handCardsCollectionView.GetSubView(index))
                .PlayNormalAnim(CancellationToken.None).Forget();
            pendingUseCardsController.AddCardView(index, interactionType); // 传递操作类型
            SetIgnoreLayout(index, true);
        }
        public void HandlingUseCard(int index)
        {
            pendingUseCardsController.RemoveCardView(index);
        }
        public void RemoveAllPendingCards()
        {
            pendingUseCardsController.RemoveAll();
        }
    }
}