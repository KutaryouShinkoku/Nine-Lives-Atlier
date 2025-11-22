using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Model;
using Game.UI.Common.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using WanFramework.Utils;

namespace Game.UI.InGame.Shop.Components
{
    public class InGameUIShopCardView : UISubView, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {

        [SerializeField]
        private UniAnimation animBuyCard;

        [SerializeField]
        private UniAnimation animSelectCard;

        [SerializeField]
        private UniAnimation animNormal;

        public UnityEvent<InGameUIShopCardView> onCardEnter;
        public UnityEvent<InGameUIShopCardView> onCardExit;
        public UnityEvent<InGameUIShopCardView> onCardDown;
        public UnityEvent<InGameUIShopCardView> onCardUp;
        public UnityEvent<InGameUIShopCardView> onCardClick;

        public void OnPointerEnter(PointerEventData eventData) => onCardEnter?.Invoke(this);
        public void OnPointerExit(PointerEventData eventData) => onCardExit?.Invoke(this);
        public void OnPointerDown(PointerEventData eventData) => onCardDown?.Invoke(this);
        public void OnPointerUp(PointerEventData eventData) => onCardUp?.Invoke(this);
        public void OnPointerClick(PointerEventData eventData) => onCardClick?.Invoke(this);


        [SerializeField]
        private GameObject root;
        [SerializeField]
        private CommonCardUIView commonCardView;
        [SerializeField]
        private TMP_Text costText;
        public CommonCardUIView CommonCardView => commonCardView;


        protected override void InitComponents()
        {
            base.InitComponents();
            Bind(nameof(ShopCardModel.Cost), m => OnCostUpdate(((ShopCardModel)m).Cost));
        }

        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            commonCardView.DataModel = (dataModel as ShopCardModel)?.Card;
        }
        private void OnCostUpdate(int cost)
        {
            root.SetActive(cost >= 0);
            costText.text = cost.ToString();
        }

        private void StopAllAnim()
        {
            animBuyCard.Cancel();
            animSelectCard.Cancel();
            animNormal.Cancel();
        }

        /// <summary>
        /// 清空UI显示
        /// </summary>
        public void ClearCardUI()
        {
            animNormal.Play(CancellationToken.None).Forget();
        }

        public async UniTask PlayBuyUseCardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animBuyCard.Play(token);
        }
        public async UniTask PlaySelectCardAnim(CancellationToken token)
        {
            StopAllAnim();
            await animSelectCard.Play(token);
        }
        public async UniTask PlayNormalAnim(CancellationToken token)
        {
            StopAllAnim();
            await animNormal.Play(token);
        }
    }
}