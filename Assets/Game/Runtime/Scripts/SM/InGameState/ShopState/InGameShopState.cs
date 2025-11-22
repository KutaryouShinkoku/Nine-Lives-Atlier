using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Audio;
using Game.Data;
using Game.Logic;
using Game.Model;
using Game.Tutorial;
using Game.UI.Common;
using Game.UI.InGame.Shop;
using UnityEngine;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState.ShopState
{
    /// <summary>
    /// 商店阶段
    /// </summary>
    public sealed class InGameShopState : InGameShopStateBase
    {
        private UnityAction<int> _onSelectCard;
        private UnityAction _onBuyCard;
        private UnityAction _onRefreshCard;
        private UnityAction _onUpgradeCard;
        private UnityAction _onLeave;

        private int _selectedCardIndex = -1;
        
        //跑教程的
        private readonly TutorialRunner _runner = new();
        private CancellationTokenSource _cts;
        
        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onSelectCard = OnSelectCard;
            _onBuyCard = OnBuyCard;
            _onRefreshCard = OnRefreshCard;
            _onUpgradeCard = OnUpgradeCard;
            _onLeave = OnLeave;
        }
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            //切商店bgm
            AudioSystem.Instance.SendEvent(AudioIds.BGM_Tavern);
            
            base.OnEnter(machine, previousState);
            _cts = new();

            // 随机下一关 LevelId
            var model = DataModel<InGameModel>.Instance;
            var currentEntry = model.CurrentLevel.Data();
            var nextPool = currentEntry.NextLevelPool.Data();
            var nextLevelId = Algorithm.Roulette(nextPool.Levels, nextPool.Weights);
            model.NextLevel = nextLevelId;

            // 随机下一关的 BossId
            var nextLevelEntry = nextLevelId.Data();
            var bossPool = nextLevelEntry.BossEnemyPool.Data();
            var nextBossId = Algorithm.Roulette(bossPool.Enemies, bossPool.Weights);
            model.NextBoss = nextBossId;

            // 把 Boss 信息推给 UI
            var shopView = UISystem.Instance.ShowUI<InGameUIShopView>("InGame/Shop/UIShop");
            var bossData = nextBossId.Data();
            shopView.SetNextBossInfo(nextLevelId);


            shopView.SetSelectedCard(null);
            shopView.SetBuyCostPreview(0);
            _selectedCardIndex = -1;
            shopView.onSelectCard.AddListener(_onSelectCard);
            shopView.onBuyButtonPressed.AddListener(_onBuyCard);
            shopView.onRefreshButtonPressed.AddListener(_onRefreshCard);
            shopView.onUpgradeButtonPressed.AddListener(_onUpgradeCard);
            shopView.onLeaveButtonPressed.AddListener(_onLeave);
            shopView.DataModel = DataModel<ShopModel>.Instance;
            //第一次进入商店界面执行
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Shop_Begin))
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_Shop_Begin);
                _runner.Run(TutorialIds.Tutorial_Shop_Begin, _cts.Token);
            }

            EnsureCoverTopmost();
        }
        private void OnSelectCard(int cardModelId)
        {
            _selectedCardIndex = cardModelId;
            var shopView = UISystem.Instance.GetUI<InGameUIShopView>("InGame/Shop/UIShop");
            var rewardCost = DataModel<ShopModel>.Instance.Cards[_selectedCardIndex];
            shopView.SetSelectedCard(rewardCost.Card);
            shopView.SetBuyCostPreview(rewardCost.Cost);
        }
        private void OnBuyCard()
        {
            if (_selectedCardIndex == -1) return;
            if (DataModel<InGameModel>.Instance.Gold < DataModel<ShopModel>.Instance.Cards[_selectedCardIndex].Cost)
            {
                Debug.Log("你钱不够了");
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.ShopNotEnoughMoney));
                return;
            }
            DataModel<InGameModel>.Instance.Gold -= DataModel<ShopModel>.Instance.Cards[_selectedCardIndex].Cost;
            DataModel<InGameModel>.Instance.CardDeck.Add(DataModel<ShopModel>.Instance.Cards[_selectedCardIndex].Card.Clone());
            var shopView = UISystem.Instance.GetUI<InGameUIShopView>("InGame/Shop/UIShop");
            shopView.SetCardInteractable(_selectedCardIndex, false);
            var hideCardIndex = _selectedCardIndex;
            shopView.PlayBuyCardAnim(_selectedCardIndex).ContinueWith(() =>
            {
                DataModel<ShopModel>.Instance.Cards[hideCardIndex].Cost = -1;
            });
            shopView.SetSelectedCard(null);
            shopView.SetBuyCostPreview(0);
            _selectedCardIndex = -1;
        }
        private void OnRefreshCard()
        {
            if (ShopLogic.TryRefreshShop(out var cost))
            {
                Debug.Log("刷新成功");
            }
            else
            {
                Debug.Log($"金币不足，需要{cost}金币");
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.ShopNotEnoughMoney));
            }
        }
        private void OnUpgradeCard()
        {
            GameManager.Current.EnterState<InGameShopChooseCardActionState>();
        }
        private void OnLeave()
        {
            //离开商店的时候，切成主菜单bgm
            AudioSystem.Instance.SendEvent(AudioIds.BGM_Mainmenu);
            
            if (LevelLogic.TryGotoNextLevel())
                GameManager.Current.EnterState<InGameChooseLevelState>();
            else
            {
                Debug.Log("There is no level after shop? Impossible!");
                GameManager.Current.EnterState<MainMenuState>();
            }
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            var shopView = UISystem.Instance.GetUI<InGameUIShopView>("InGame/Shop/UIShop");
            shopView.onSelectCard.RemoveListener(_onSelectCard);
            shopView.onBuyButtonPressed.RemoveListener(_onBuyCard);
            shopView.onRefreshButtonPressed.RemoveListener(_onRefreshCard);
            shopView.onUpgradeButtonPressed.RemoveListener(_onUpgradeCard);
            shopView.onLeaveButtonPressed.RemoveListener(_onLeave);
            base.OnExit(machine, nextState);
        }
    }
}