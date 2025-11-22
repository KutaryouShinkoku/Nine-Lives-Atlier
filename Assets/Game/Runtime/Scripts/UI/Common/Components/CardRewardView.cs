using UnityEngine;
using WanFramework.UI;
using WanFramework.Utils;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Game.Data;
using TMPro;
using WanFramework.UI.DataComponent;

namespace Game.UI.Common.Components
{
    public class CardRewardView : UISubView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField]
        private UniAnimation animSelectReward;
        [SerializeField]
        private UniAnimation animShowReward;
        [SerializeField]
        private UniAnimation animHideReward;
        [SerializeField]
        private UniAnimation animHideChosenReward;
        [SerializeField]
        private UniAnimation animNormal;

        [SerializeField]
        private CommonCardUIView root;

        public UnityEvent<CardRewardView> onCardEnter;
        public UnityEvent<CardRewardView> onCardExit;
        public UnityEvent<CardRewardView> onCardDown;
        public UnityEvent<CardRewardView> onCardUp;
        public UnityEvent<CardRewardView> onCardClick;

        public bool IsMouseOver { get; private set; }


        public void SetCard(CardIds id) => root.SetCard(id);

        public void OnPointerEnter(PointerEventData eventData) => onCardEnter?.Invoke(this);
        public void OnPointerExit(PointerEventData eventData) => onCardExit?.Invoke(this);
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onCardDown?.Invoke(this);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onCardUp?.Invoke(this);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onCardClick?.Invoke(this);
        }


        public void SetRootDataModel(DataModelBase dataModel)
        {
            root.DataModel = dataModel;
        }

        private void StopAllAnim()
        {
            animSelectReward.Cancel();
            animShowReward.Cancel();
            animHideReward.Cancel();
            animHideChosenReward.Cancel();
            animNormal.Cancel();
        }

        public async UniTask PlayNormalAnim(CancellationToken token)
        {
            StopAllAnim();
            await animNormal.Play(token);
        }

        public async UniTask PlaySelectRewardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animSelectReward.Play(token);
        }
        public async UniTask PlayShowRewardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animShowReward.Play(token);
        }
        public async UniTask PlayHideRewardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animHideReward.Play(token);
        }
        public async UniTask PlayHideChosenRewardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animHideChosenReward.Play(token);
        }

    }

}
