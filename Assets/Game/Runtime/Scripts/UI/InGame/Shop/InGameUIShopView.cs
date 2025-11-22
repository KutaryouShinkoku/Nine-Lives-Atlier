using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.Data;
using Game.Localization.Components;
using Game.Logic;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.Common.Components;
using Game.UI.InGame.Shop.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.InGame.Shop
{
    public class InGameUIShopView : UIRootView
    {
        [SerializeField]
        private InGameUIShopCardView[] cardViews;
        private bool[] _cardInteractableStates;
        [SerializeField]
        private TMP_Text actionCostText;
        [SerializeField]
        private Button refreshButton;
        [SerializeField]
        private Button buyButton;
        [SerializeField]
        private Button upgradeButton;
        [SerializeField]
        private Button leaveButton;

        [SerializeField]
        private GameObject buyPreviewRoot;
        [SerializeField]
        private TMP_Text buyCostText;
        
        [SerializeField]
        private TMP_Text boardDeleteCostText;
        [SerializeField]
        private TMP_Text boardUpgradeCostText;
        [SerializeField]
        private TMP_Text boardRefreshCostText;
        [SerializeField]
        private TMP_Text boardDeleteCostAdditionText;
        [SerializeField]
        private TMP_Text boardUpgradeCostAdditionText;
        [SerializeField]
        private TMP_Text boardRefreshCostAdditionText;
        
        [SerializeField]
        private CommonCardUIView selectedCardView;

        [SerializeField]
        private int _lastHoverCard = -1;
        
        public UnityEvent<int> onSelectCard;
        public UnityEvent onRefreshButtonPressed;
        public UnityEvent onBuyButtonPressed;
        public UnityEvent onUpgradeButtonPressed;
        public UnityEvent onLeaveButtonPressed;

        [Header("显示下一个Boss的信息")]
        [SerializeField] private LocalizeText nextLevelDesc;
        [SerializeField] private LocalizeText nextBossDesc;

        [SerializeField] private Image level1Img;
        [SerializeField] private Image level2Img;
        [SerializeField] private Image level3Img;
        [SerializeField] private Image level4Img;
        [SerializeField] private Image level5Img;

        public int CardCount => cardViews.Length;

        private static void SetFullCostText(int cost, int nextCost, TMP_Text outText, TMP_Text outAdditionText)
        {
            outText.text = cost.ToString();
            outAdditionText.text = (nextCost - cost).ToString();
        }
        protected override void InitComponents()
        {
            base.InitComponents();
            _cardInteractableStates = new bool[cardViews.Length];
            for (var i = 0; i < cardViews.Length; ++i)
            {
                var index = i;
                cardViews[i].onCardClick.AddListener(_ =>
                {
                    if (_cardInteractableStates[index])
                        onSelectCard?.Invoke(index);
                });
                cardViews[i].onCardEnter.AddListener(_ =>
                {
                    if (_cardInteractableStates[index])
                        OnHoverCard(index);
                });
                cardViews[i].onCardExit.AddListener(_ =>
                {
                    OnHoverCard(-1);
                });
            }
            refreshButton.onClick.AddListener(() => onRefreshButtonPressed?.Invoke());
            buyButton.onClick.AddListener(() => onBuyButtonPressed?.Invoke());
            upgradeButton.onClick.AddListener(() => onUpgradeButtonPressed?.Invoke());
            leaveButton.onClick.AddListener(() => onLeaveButtonPressed?.Invoke());
            Bind(nameof(ShopModel.RefreshCost), m => actionCostText.text = (m as ShopModel)?.RefreshCost.ToString() ?? string.Empty);
            Bind(nameof(ShopModel.DeleteCost), m =>
                SetFullCostText(((ShopModel)m).DeleteCost, ShopLogic.GetNextDeleteCost(), boardDeleteCostText, boardDeleteCostAdditionText));
            Bind(nameof(ShopModel.RefreshCost), m =>
                SetFullCostText(((ShopModel)m).RefreshCost, ShopLogic.GetNextRefreshCost(), boardRefreshCostText, boardRefreshCostAdditionText));
            Bind(nameof(ShopModel.UpgradeCost), m =>
                SetFullCostText(((ShopModel)m).UpgradeCost, ShopLogic.GetNextUpgradeCost(1), boardUpgradeCostText, boardUpgradeCostAdditionText));
        }
        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            var shopModel = dataModel as ShopModel;
            for (var i = 0; i < cardViews.Length; ++i)
                cardViews[i].DataModel = shopModel?.Cards[i];
        }
        private void OnHoverCard(int cardIndex)
        {
            if (_lastHoverCard != -1)
                cardViews[_lastHoverCard].PlayNormalAnim(CancellationToken.None).Forget();
            if (cardIndex != -1)
                cardViews[cardIndex].PlaySelectCardAnim(CancellationToken.None).Forget();
            _lastHoverCard = cardIndex;
        }
        public InGameUIShopCardView GetCardView(int cardIndex) => cardViews[cardIndex];
        public void SetCardInteractable(int cardIndex, bool interactable)
        {
            _cardInteractableStates[cardIndex] = interactable;
            if (!interactable && _lastHoverCard == cardIndex)
                cardViews[cardIndex].PlayNormalAnim(CancellationToken.None).Forget();
        }

        public async UniTask PlayBuyCardAnim(int cardIndex)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Shop_Buy);
            await cardViews[cardIndex].PlayBuyUseCardAnim(CancellationToken.None);
        }

        public void PlayAllCardNormalAnim()
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Shop_Refresh);
            for (int i = 0; i < cardViews.Length; i++)
            {
                cardViews[i].PlayNormalAnim(CancellationToken.None).Forget();
            }
        }

        public void SetSelectedCard(CardModel card)
        {
            if (card == null)
                selectedCardView.gameObject.SetActive(false);
            else
            {
                selectedCardView.gameObject.SetActive(true);
                selectedCardView.DataModel = card;
            }
        }
        public void SetBuyCostPreview(int cost)
        {
            if (cost <= 0)
                buyPreviewRoot.SetActive(false);
            else
            {
                buyPreviewRoot.SetActive(true);
                buyCostText.text = cost.ToString();
            }
        }

        /// <summary>
        /// 显示下一关 Boss的名称和简介
        /// </summary>
        public void SetNextBossInfo(LevelIds level)
        {
            nextLevelDesc.SetText(level.Data().Name);
            nextBossDesc.SetText(level.Data().BossDesc);
            SetNextLevelIcon(level);
        }

        private void SetNextLevelIcon(LevelIds level)
        {
            switch (level)
            {
                case LevelIds.Level1:
                    level1Img.gameObject.SetActive(true);
                    level2Img.gameObject.SetActive(false);
                    level3Img.gameObject.SetActive(false);
                    level4Img.gameObject.SetActive(false);
                    level5Img.gameObject.SetActive(false);
                    break;
                case LevelIds.Level2:
                    level1Img.gameObject.SetActive(false);
                    level2Img.gameObject.SetActive(true);
                    level3Img.gameObject.SetActive(false);
                    level4Img.gameObject.SetActive(false);
                    level5Img.gameObject.SetActive(false);
                    break;
                case LevelIds.Level3:
                    level1Img.gameObject.SetActive(false);
                    level2Img.gameObject.SetActive(false);
                    level3Img.gameObject.SetActive(true);
                    level4Img.gameObject.SetActive(false);
                    level5Img.gameObject.SetActive(false);
                    break;
                case LevelIds.Level4:
                    level1Img.gameObject.SetActive(false);
                    level2Img.gameObject.SetActive(false);
                    level3Img.gameObject.SetActive(false);
                    level4Img.gameObject.SetActive(true);
                    level5Img.gameObject.SetActive(false);
                    break;
                case LevelIds.Level5:
                    level1Img.gameObject.SetActive(false);
                    level2Img.gameObject.SetActive(false);
                    level3Img.gameObject.SetActive(false);
                    level4Img.gameObject.SetActive(false);
                    level5Img.gameObject.SetActive(true);
                    break;
                default:
                    level1Img.gameObject.SetActive(false);
                    level2Img.gameObject.SetActive(false);
                    level3Img.gameObject.SetActive(false);
                    level4Img.gameObject.SetActive(false);
                    level5Img.gameObject.SetActive(true);
                    break;
            }
        }
    }
}