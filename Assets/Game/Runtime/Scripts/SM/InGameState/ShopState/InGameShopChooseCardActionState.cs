using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Logic;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.Tutorial;
using Game.UI.Common;
using Game.UI.InGame.Shop;
using UnityEngine;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.SM;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.SM.InGameState.ShopState
{
    public enum ShopCardAction
    {
        None, Upgrade, Delete
    }
    public class InGameShopChooseCardActionState : InGameStateBase
    {
        private UnityAction _onCancel;
        private UnityAction<int> _onSelect;
        
        private UnityAction _onCancelAction;
        private UnityAction _onConfirmAction;
        private UnityAction _onDelete;
        private UnityAction _onAdd;
        private UnityAction _onMinus;
        private UnityAction<int> _onSelectEffectVal;
        
        private int _selected = -1;
        private int _selectedEffectIndex = -1;
        private int _selectedCardIndex;
        private readonly int[] _cardEffectVals = new int[3];

        //跑教程的
        private readonly TutorialRunner _runner = new();
        private CancellationTokenSource _cts;
        
        private ShopCardAction _currentAction;
        
        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onCancel = OnCancel;
            _onSelect = OnSelect;
            _onConfirmAction = OnConfirmAction;
            _onCancelAction = OnCancelAction;
            _onDelete = OnDelete;
            _onAdd = OnAdd;
            _onMinus = OnMinus;
            _onSelectEffectVal = OnSelectEffectVal;
        }
        private int GetEffectAdditionValByIndex(CardModel card, int effectIndex) => effectIndex switch
        {
            1 => card.AdditionEffectVal1,
            2 => card.AdditionEffectVal2,
            3 => card.AdditionEffectVal3,
            _ => -1
        };
        private int GetEffectBaseValByIndex(CardModel card, int index) => card.Id.Data().EffectValue[index - 1];
        private int GetCurrentCost()
        {
            int cost;
            switch (_currentAction)
            {
                case ShopCardAction.None:
                    cost = 0;
                    break;
                case ShopCardAction.Upgrade:
                    var card = DataModel<InGameModel>.Instance.CardDeck[_selectedCardIndex];
                    var shopModel = DataModel<ShopModel>.Instance;
                    cost = ShopLogic.CalculateUpgradeCostWithDelta(card, _cardEffectVals, shopModel.UpgradeCost).cost;
                    break;
                case ShopCardAction.Delete:
                    cost = DataModel<ShopModel>.Instance.DeleteCost;
                    break;
                default:
                    cost = -1;
                    break;
            }
            return cost;
        }

        private void OnSelectEffectVal(int effectValId)
        {
            if (_currentAction == ShopCardAction.Delete) return;
            var upgradeCardView = UISystem.Instance.GetUI<InGameUIUpgradeCardView>("InGame/Shop/UIShopUpgradeCard");
            upgradeCardView.SetSelectedEffectVal(effectValId);
            _selectedEffectIndex = effectValId;
        }
        private void UpdateView()
        {
            var card = _selectedCardIndex < 0 ? null : DataModel<InGameModel>.Instance.CardDeck[_selectedCardIndex];
            var upgradeCardView = UISystem.Instance.GetUI<InGameUIUpgradeCardView>("InGame/Shop/UIShopUpgradeCard");
            upgradeCardView.SetCancelButtonEnable(true);
            upgradeCardView.SetCost(GetCurrentCost());
            if (_currentAction == ShopCardAction.Upgrade)
                upgradeCardView.SetEffectVals(
                    _cardEffectVals[0], 
                    _cardEffectVals[1], 
                    _cardEffectVals[2]);
            else
                upgradeCardView.SetEffectVals(
                    card?.AdditionEffectVal1 ?? 0, 
                    card?.AdditionEffectVal2 ?? 0, 
                    card?.AdditionEffectVal3 ?? 0);
            upgradeCardView.SetCurrentAction(_currentAction);
            upgradeCardView.SetSelectedEffectVal(_currentAction == ShopCardAction.Delete ? -1 : _selectedEffectIndex);
            upgradeCardView.SetConfirmButtonEnable(_currentAction != ShopCardAction.None);
        }
        private void HideActionView()
        {
            UISystem.Instance.Hide("InGame/Shop/UIShopUpgradeCard");
            _selectedCardIndex = -1;
            var selectCardView = UISystem.Instance.GetUI<InGameUIShopSelectCardDeckView>("InGame/Shop/UIShopSelectCardDeck");
            _currentAction = ShopCardAction.None;
            _selected = -1;
        }
        #region 牌库区选牌交互
        private void OnCancel()
        {
            GameManager.Current.EnterState<InGameShopState>();
        }
        private void OnSelect(int cardIndex)
        {
            _selected = cardIndex;
            if (_selected >= 0)
                OnChooseCard(_selected);
        }
        #endregion

        #region 升级区交互
        private void OnMinus()
        {
            if (_selectedEffectIndex < 0)
            {
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.ShopNoValSelected));
                return;
            }
            var card = DataModel<InGameModel>.Instance.CardDeck[_selectedCardIndex];
            --_cardEffectVals[_selectedEffectIndex - 1];
            if (_cardEffectVals[_selectedEffectIndex - 1] + GetEffectBaseValByIndex(card, _selectedEffectIndex) < 0)
                _cardEffectVals[_selectedEffectIndex - 1] = -GetEffectBaseValByIndex(card, _selectedEffectIndex);
            OnEffectValueChanged();
            UpdateView();
        }
        private void OnAdd()
        {
            if (_selectedEffectIndex < 0)
            {
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.ShopNoValSelected));
                return;
            }
            ++_cardEffectVals[_selectedEffectIndex - 1];
            OnEffectValueChanged();
            UpdateView();
        }
        private void OnEffectValueChanged()
            => _currentAction = HasCardEffectUpgrade() ? ShopCardAction.Upgrade : ShopCardAction.None;
        private bool HasCardEffectUpgrade()
        {
            var card = DataModel<InGameModel>.Instance.CardDeck[_selectedCardIndex];
            return _cardEffectVals[0] != GetEffectAdditionValByIndex(card, 1) ||
                   _cardEffectVals[1] != GetEffectAdditionValByIndex(card, 2) ||
                   _cardEffectVals[2] != GetEffectAdditionValByIndex(card, 3);
        }
        private void OnConfirmAction()
        {
            var upgradeCardView = UISystem.Instance.GetUI<InGameUIUpgradeCardView>("InGame/Shop/UIShopUpgradeCard");
            upgradeCardView.SetCancelButtonEnable(false);
            
            switch (_currentAction)
            {
                case ShopCardAction.None:
                    break;
                case ShopCardAction.Upgrade:
                    if (OnConfirmUpgrade())
                        upgradeCardView.PlayUpgradeAnimation(_cts.Token).ContinueWith(HideActionView).Forget();
                    break;
                case ShopCardAction.Delete:
                    if (OnConfirmDelete())
                        upgradeCardView.PlayDeleteAnimation(_cts.Token).ContinueWith(HideActionView).Forget();
                    break;
            }
        }
        private void OnCancelAction()
        {
            switch (_currentAction)
            {
                case ShopCardAction.None:
                    UISystem.Instance.Hide("InGame/Shop/UIShopUpgradeCard");
                    break;
                case ShopCardAction.Upgrade:
                    _currentAction = ShopCardAction.None;
                    for (var i = 0; i < _cardEffectVals.Length; ++i) _cardEffectVals[i] = 0;
                    UpdateView();
                    break;
                case ShopCardAction.Delete:
                    //删除状态取消也清除存储的升级数据，直接跳转到None状态
                    _currentAction = ShopCardAction.None;
                    for (var i = 0; i < _cardEffectVals.Length; ++i) _cardEffectVals[i] = 0;
                    UpdateView();
                    break;
            }
        }
        private void OnDelete()
        {
            if (_currentAction == ShopCardAction.Delete)
            {
                _currentAction = ShopCardAction.None;
                for (var i = 0; i < _cardEffectVals.Length; ++i) _cardEffectVals[i] = 0;
            }
            else
                _currentAction = ShopCardAction.Delete;
            UpdateView();
        }
        private bool OnConfirmDelete()
        {
            if (!ShopLogic.TryDeleteCard(_selectedCardIndex, out var cost))
            {
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.ShopNotEnoughMoney));
                Debug.Log($"金币不足，需要{cost}金币");
                return false;
            }
            return true;
        }
        private bool OnConfirmUpgrade()
        {
            if (!ShopLogic.TryUpgradeCard(_selectedCardIndex, _cardEffectVals, out var cost))
            {
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.ShopNotEnoughMoney));
                Debug.Log($"金币不足，需要{cost}金币");
                return false;
            }
            return true;
        }
        #endregion
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            _cts = new();
            var selectCardView = UISystem.Instance.ShowUI<InGameUIShopSelectCardDeckView>("InGame/Shop/UIShopSelectCardDeck");
            selectCardView.onCancel.AddListener(_onCancel);
            selectCardView.onSelectCardDeck.AddListener(_onSelect);
            selectCardView.DataModel = DataModel<InGameModel>.Instance;
            var upgradeCardView = UISystem.Instance.GetUI<InGameUIUpgradeCardView>("InGame/Shop/UIShopUpgradeCard");
            upgradeCardView.onConfirm.AddListener(_onConfirmAction);
            upgradeCardView.onCancel.AddListener(_onCancelAction);
            upgradeCardView.onAdd.AddListener(_onAdd);
            upgradeCardView.onMinus.AddListener(_onMinus);
            upgradeCardView.onDelete.AddListener(_onDelete);
            upgradeCardView.onChooseEffectVal.AddListener(_onSelectEffectVal);
            upgradeCardView.DataModel = DataModel<ShopModel>.Instance;
            for (var i = 0; i < _cardEffectVals.Length; ++i) _cardEffectVals[i] = 0;
            _selected = -1;
            _currentAction = ShopCardAction.None;

            EnsureCoverTopmost();
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            var selectCardView = UISystem.Instance.GetUI<InGameUIShopSelectCardDeckView>("InGame/Shop/UIShopSelectCardDeck");
            selectCardView.onCancel.RemoveListener(_onCancel);
            selectCardView.onSelectCardDeck.RemoveListener(_onSelect);
            _selected = -1;
            UISystem.Instance.Hide(selectCardView);
            var upgradeCardView = UISystem.Instance.GetUI<InGameUIUpgradeCardView>("InGame/Shop/UIShopUpgradeCard");
            upgradeCardView.onConfirm.RemoveListener(_onConfirmAction);
            upgradeCardView.onCancel.RemoveListener(_onCancelAction);
            upgradeCardView.onAdd.RemoveListener(_onAdd);
            upgradeCardView.onMinus.RemoveListener(_onMinus);
            upgradeCardView.onDelete.RemoveListener(_onDelete);
            upgradeCardView.onChooseEffectVal.RemoveListener(_onSelectEffectVal);
            UISystem.Instance.Hide(upgradeCardView);
        }
        private void OnChooseCard(int cardDeckIndex)
        {
            _selectedCardIndex = cardDeckIndex;
            var card = DataModel<InGameModel>.Instance.CardDeck[_selectedCardIndex];
            var upgradeCardView = UISystem.Instance.ShowUI<InGameUIUpgradeCardView>("InGame/Shop/UIShopUpgradeCard");
            upgradeCardView.SetCard(card);
            upgradeCardView.SetCost(0);
            upgradeCardView.SetSelectedEffectVal(-1);
            _selectedEffectIndex = -1;
            _cardEffectVals[0] = card.AdditionEffectVal1;
            _cardEffectVals[1] = card.AdditionEffectVal2;
            _cardEffectVals[2] = card.AdditionEffectVal3;
            UpdateView();
            //第一次升级卡牌时运行
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_ShopUpdate_Begin))
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_ShopUpdate_Begin);
                _runner.Run(TutorialIds.Tutorial_ShopUpdate_Begin, _cts.Token);
            }

            EnsureCoverTopmost();
        }
    }
}