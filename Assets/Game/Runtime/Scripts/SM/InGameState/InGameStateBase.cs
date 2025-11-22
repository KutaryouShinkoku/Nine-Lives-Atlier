using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.InGame;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState
{
    /// <summary>
    /// 局内阶段，用于处理统一UI的事件
    /// </summary>
    public abstract class InGameStateBase : GameState
    {
        private UnityAction<DataModelBase, string> _onCardDeckChanged;
        private UnityAction<DataModelBase, string> _onDrawDeckChanged;
        private UnityAction<DataModelBase, string> _onDiscardDeckChanged;
        
        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onCardDeckChanged = (m, s) =>
                UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover")
                    .SetCardDeckCount(m.As<DataCollection<CardModel>>().Count);
            _onDrawDeckChanged = (m, s) =>
                UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover")
                    .SetDrawDeckCount(m.As<DataCollection<CardModel>>().Count);
            _onDiscardDeckChanged = (m, s) =>
                UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover")
                    .SetDiscardDeckCount(m.As<DataCollection<CardModel>>().Count);
        }
        
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            var ui = UISystem.Instance.IsShowing("InGame/UICover") ? 
                UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover") : 
                UISystem.Instance.ShowUI<InGameUICoverView>("InGame/UICover");
            ui.DataModel = DataModel<InGameModel>.Instance;
            ui.onSettingsClick.AddListener(OnSettings);
            SetCardDeckButtonVisible(true);
            DataModel<BattleModel>.Instance.RemainCards.onPropertyChanged.AddListener(_onDrawDeckChanged);
            DataModel<BattleModel>.Instance.DiscardCards.onPropertyChanged.AddListener(_onDiscardDeckChanged);
            DataModel<InGameModel>.Instance.CardDeck.onPropertyChanged.AddListener(_onCardDeckChanged);
            _onCardDeckChanged?.Invoke(DataModel<BattleModel>.Instance.RemainCards, "");
            _onDiscardDeckChanged?.Invoke(DataModel<BattleModel>.Instance.DiscardCards, "");
            _onCardDeckChanged?.Invoke(DataModel<InGameModel>.Instance.CardDeck, "");
        }

        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            if (nextState is not InGameStateBase)
            {
                DataModel<BattleModel>.Instance.RemainCards.onPropertyChanged.RemoveListener(_onDrawDeckChanged);
                DataModel<BattleModel>.Instance.DiscardCards.onPropertyChanged.RemoveListener(_onDiscardDeckChanged);
                DataModel<InGameModel>.Instance.CardDeck.onPropertyChanged.RemoveListener(_onCardDeckChanged);
                UISystem.Instance.Hide("InGame/UICover");
                UISystem.Instance.Hide("InGame/UIPause");
                UISystem.Instance.Hide("Common/UICardDeck");
            }
        }

        protected void EnsureCoverTopmost() => UISystem.Instance.MoveToTop("InGame/UICover");
        private void OnSettings()
        {
            UISystem.Instance.ShowUI("InGame/UIPause");
        }
        protected void SetCardDeckButtonVisible(bool visible) =>
            UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover").SetCardDeckVisible(visible);
        protected void SetDrawCardButtonVisible(bool visible) =>
            UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover").SetDrawCardVisible(visible);
        protected void SetDiscardCardButtonVisible(bool visible) =>
            UISystem.Instance.GetUI<InGameUICoverView>("InGame/UICover").SetDiscardCardVisible(visible);
    }
}