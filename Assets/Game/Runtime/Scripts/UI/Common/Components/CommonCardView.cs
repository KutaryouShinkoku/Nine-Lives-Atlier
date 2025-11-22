using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.Data;
using Game.Logic;
using Game.Model.InGameSubModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game.UI.Common.Components
{
    public class CommonCardView : SubView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerMoveHandler
    {
        [SerializeField]
        private UniAnimation animDrawCard;

        [SerializeField]
        private UniAnimation animSelectCard;

        [SerializeField]
        private UniAnimation animUnselectCard;

        [SerializeField]
        private UniAnimation animSacrificeCard;

        [SerializeField]
        private UniAnimation animUseCard;

        [SerializeField]
        private UniAnimation animDraggingCard;

        [SerializeField]
        private UniAnimation animDraggingUseCard;

        [SerializeField]
        private UniAnimation animDraggingSacrificeCard;

        [SerializeField]
        private UniAnimation animDiscard;

        [SerializeField]
        private UniAnimation animNormal;

        public UnityEvent<CommonCardView> onCardEnter;
        public UnityEvent<CommonCardView> onCardExit;
        public UnityEvent<CommonCardView, int> onCardDown;
        public UnityEvent<CommonCardView> onCardUp;
        public UnityEvent<CommonCardView, int> onCardClick;
        
        [SerializeField]
        private CommonCardUIView root;
        
        [SerializeField]
        private Canvas rootCanvas;
        
        [FormerlySerializedAs("clickMaxMouseMove")]
        [SerializeField]
        private float clickMaxMouseMovement = 5f;

        private float _currentMouseMovement = 0;
        
        private Collider _collider;
        
        public bool IsMouseOver { get; private set; }

        public bool IsVisible
        {
            get => root.gameObject.activeSelf;
            set => root.gameObject.SetActive(value);
        }
        public bool Interactable
        {
            get => _collider?.enabled ?? false;
            set
            {
                if (_collider) _collider.enabled = value;
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            _currentMouseMovement += clickMaxMouseMovement + 1;
        }
        protected override void InitComponents()
        {
            TryGetComponent(out _collider);
            base.InitComponents();
            rootCanvas.worldCamera = GameManager.Current.MainCamera;
        }

        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            root.DataModel = DataModel;
        }

        /// <summary>
        /// 设置并绑定卡牌模型
        /// </summary>
        /// <param name="id">卡牌ID</param>
        public void SetCard(CardIds id) => root.SetCard(id);
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            onCardEnter?.Invoke(this);
            IsMouseOver = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            onCardExit?.Invoke(this);
            IsMouseOver = false;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            var btnId = eventData.button switch
            {
                PointerEventData.InputButton.Left => 0,
                PointerEventData.InputButton.Right => 1,
                _ => -1
            };
            onCardDown?.Invoke(this, btnId);
            _currentMouseMovement = 0;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            onCardUp?.Invoke(this);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            var btnId = eventData.button switch
            {
                PointerEventData.InputButton.Left => 0,
                PointerEventData.InputButton.Right => 1,
                _ => -1
            };
            if (_currentMouseMovement <= clickMaxMouseMovement && btnId != -1)
                onCardClick?.Invoke(this, btnId);
        }
        public void OnPointerMove(PointerEventData eventData)
        {
            _currentMouseMovement += eventData.delta.magnitude;
        }

        /// <summary>
        /// 清空UI显示
        /// </summary>
        public void ClearCardUI()
        {
            animNormal.Play(CancellationToken.None).Forget();
        }

        public async UniTask PlayShowAnim(CancellationToken token)
        {
            StopAllAnim();
            await animDrawCard.Play(token);
        }
        public async UniTask PlaySelectAnim(CancellationToken token)
        {
            StopAllAnim();
            await animSelectCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Selected);
            AudioSystem.Instance.SendEvent(AudioIds.CardEffect_Fadeout);
        }
        public async UniTask PlayUnselectAnim(CancellationToken token)
        {
            StopAllAnim();
            await animUnselectCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Card_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.CardEffect_Fadeout);
        }
        public async UniTask PlaySacrificeCardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animSacrificeCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Card_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.CardEffect_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Transmute);
        }
        public async UniTask PlayUseCardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animUseCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Card_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.CardEffect_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Use_Normal);
        }
        public async UniTask PlayDraggingAnim(CancellationToken token)
        {
            StopAllAnim();
            await animDraggingCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Drag);
        }
        public async UniTask PlayDraggingUseAnim(CancellationToken token)
        {
            StopAllAnim();
            await animDraggingUseCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Drag_Use);
        }
        public async UniTask PlayDraggingSacrificeAnim(CancellationToken token)
        {
            StopAllAnim();
            await animDraggingSacrificeCard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Drag_Transmute); 
        }
        public async UniTask PlayDiscardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animDiscard.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Card_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.CardEffect_Fadeout);
        }
        public async UniTask PlayNormalAnim(CancellationToken token)
        {
            StopAllAnim();
            await animNormal.Play(token);
            AudioSystem.Instance.SendEvent(AudioIds.Card_Fadeout);
            AudioSystem.Instance.SendEvent(AudioIds.CardEffect_Fadeout);
        }

        private void StopAllAnim()
        {
            animDrawCard.Cancel();
            animSelectCard.Cancel();
            animUnselectCard.Cancel();
            animSacrificeCard.Cancel();
            animDraggingCard.Cancel();
            animDraggingSacrificeCard.Cancel();
            animDraggingUseCard.Cancel();
            animDiscard.Cancel();
            animNormal.Cancel();
        }
    }
}
