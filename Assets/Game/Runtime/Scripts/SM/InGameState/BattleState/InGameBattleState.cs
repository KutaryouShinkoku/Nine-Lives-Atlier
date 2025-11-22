using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Data;
using Game.Logic;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.Tutorial;
using Game.UI.Common;
using Game.UI.InGame;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.SM;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState.BattleState
{
    public enum PlayerInteractionType
    {
        None, UseCard, SacrificeCard, DrawCard
    }
    /// <summary>
    /// 战斗阶段
    /// </summary>
    public sealed class InGameBattleState : InGameBattleStateBase
    {
        [CanBeNull]
        private CancellationTokenSource _cts;
        private UnityAction _onPlayerDrawCard;
        private UnityAction<int> _onPlayerUseCard;
        private UnityAction<int> _onPlayerSacrificeCard;
        
        private UniTask _currentAnimTask;

        //跑教程的
        private readonly TutorialRunner _runner = new();

        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onPlayerDrawCard = OnPlayerDrawCard;
            _onPlayerUseCard = OnPlayerUseCard;
            _onPlayerSacrificeCard = OnPlayerSacrificeCard;
        }
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            _cts = new();
            var handAreaView = UISystem.Instance.ShowCommonView<InGameHandAreaView>("InGame/HandAreaView");
            //修改HandAreaView的绑定model为BattleModel
            handAreaView.DataModel = DataModel<BattleModel>.Instance;
            handAreaView.onDrawCard.AddListener(_onPlayerDrawCard);
            handAreaView.onUseCard.AddListener(_onPlayerUseCard);
            handAreaView.onSacrificeCard.AddListener(_onPlayerSacrificeCard);
            EnsureCoverTopmost();
            BeforePlayerInteraction();

            UISystem.Instance.ShowUI<UIPopupNumberView>("InGame/UIPopupNumber");
            SetCardDeckButtonVisible(false);
            SetDiscardCardButtonVisible(true);
            SetDrawCardButtonVisible(true);
            
            var handCardAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");   
            handCardAreaView.CanDragCard = true;
            handCardAreaView.CanDrawCard = true;
            handCardAreaView.CanSelectCard = true;

            // 第一次进Boss战
            if (DataModel<InGameModel>.Instance.LevelInnerState == LevelInnerState.Boss &&
                !DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Boss_Begin))
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_Boss_Begin);
                _runner.Run(TutorialIds.Tutorial_Boss_Begin, _cts.Token);
            }
        }

        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            var handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
            handAreaView.onDrawCard.RemoveListener(_onPlayerDrawCard);
            handAreaView.onUseCard.RemoveListener(_onPlayerUseCard);
            handAreaView.onSacrificeCard.RemoveListener(_onPlayerSacrificeCard);
            UISystem.Instance.Hide("InGame/HandAreaView");
            SetCardDeckButtonVisible(true);
            SetDiscardCardButtonVisible(false);
            SetDrawCardButtonVisible(false);
            BattleAnimSystem.Instance.ClearAnimQueue();
        }
        private void BeforePlayerInteraction()
        {
            //测血量低于25
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_HealthBelow25_Begin) && DataModel<BattleModel>.Instance.PlayerModel.Health <= 25)
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_HealthBelow25_Begin);
                _runner.Run(TutorialIds.Tutorial_HealthBelow25_Begin, _cts!.Token);
            }

            var handCardAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");   
            // handCardAreaView.CanDragCard = true;
            handCardAreaView.CanDrawCard = true;
            // handCardAreaView.CanSelectCard = true;
            // 重新显示牌库按钮
            SetDiscardCardButtonVisible(true);
            SetDrawCardButtonVisible(true);
        }
        private void OnPlayerDrawCard() => OnPlayerInteraction(PlayerInteractionType.DrawCard, -1);
        private void OnPlayerUseCard(int index) => OnPlayerInteraction(PlayerInteractionType.UseCard, index);
        private void OnPlayerSacrificeCard(int index) => OnPlayerInteraction(PlayerInteractionType.SacrificeCard, index);
        private void OnPlayerInteraction(PlayerInteractionType type, int usedCardIndex)
        {
            if (type == PlayerInteractionType.DrawCard &&
                !BattleLogic.CanDrawCard())
            {
                Debug.Log("TODO: 在这里播放卡牌抓满动画");
                //显示卡牌不足提示
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError",view => view.SetError(ErrorTipType.HandFull));
                return;
            }
            if (type == PlayerInteractionType.UseCard &&
                !BattleLogic.HasEnoughResource(
                    DataModel<BattleModel>.Instance.PlayerModel.Resources,
                    DataModel<BattleModel>.Instance.HandCards[usedCardIndex]))
            {
                //显示资源不足提示
                UISystem.Instance.ShowUI<PopupErrorView>("InGame/PopupError", view => view.SetError(ErrorTipType.InsufficientElements));
                var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
                playerView.PlayResourceShakeAnim(CancellationToken.None).Forget();
                return;
            }
            AfterPlayerInteraction(type, usedCardIndex);
        }
        private void AfterPlayerInteraction(PlayerInteractionType interaction, int usedCardIndex)
        {
            var handCardAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
            // handCardAreaView.CanDragCard = false;
            handCardAreaView.CanDrawCard = false;
            // handCardAreaView.CanSelectCard = false;
            if (interaction == PlayerInteractionType.UseCard ||
                interaction == PlayerInteractionType.SacrificeCard)
                handCardAreaView.PendingUseCard(usedCardIndex, interaction);
            BattleSettlement(interaction, usedCardIndex);
            // 同步卡牌可交互状态
            for (var i = 0; i < DataModel<BattleModel>.Instance.HandCards.Count; i++)
                handCardAreaView.SetCardInteractable(i, !BattleLogic.IsPendingRemove(i));
            if (interaction == PlayerInteractionType.UseCard ||
                interaction == PlayerInteractionType.SacrificeCard)
                handCardAreaView.SetIgnoreLayout(usedCardIndex, true);
            // 播放动画时隐藏牌库按钮
            SetDiscardCardButtonVisible(false);
            SetDrawCardButtonVisible(false);
            // 根据游戏是否结束决定可交互状态
            if (IsGameFinished())
            {
                handCardAreaView.CanDragCard = false;
                handCardAreaView.CanDrawCard = false;
                handCardAreaView.CanSelectCard = false;
            }
            // 播放战斗动画
            EnsureBattleAnimPlaying();
        }
        private void EnsureBattleAnimPlaying()
        {
            if (_currentAnimTask.Status == UniTaskStatus.Succeeded)
                _currentAnimTask = BattleAnim(_cts!.Token);
        }
        private bool IsGameFinished() =>
            DataModel<BattleModel>.Instance.PlayerModel.Health <= 0 || DataModel<BattleModel>.Instance.EnemyModel.Health <= 0;
        private void OnBattleAnimFinished()
        {
            try
            {
                if (DataModel<BattleModel>.Instance.PlayerModel.Health <= 0)
                    OnPlayerLose();
                else if (DataModel<BattleModel>.Instance.EnemyModel.Health <= 0)
                {
                    //获胜之前先播放敌人死亡动画
                    var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
                    enemyView.PlayDeathAnim(_cts!.Token).ContinueWith(OnPlayerWin).Forget();
                }
                else
                    // 回到等待玩家交互状态
                    BeforePlayerInteraction();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnPlayerWin()
        {
            Debug.Log("玩家获胜");

            //玩家获胜金币增加敌人的奖励数量
            var gameModel = DataModel<InGameModel>.Instance;
            //gameModel.PlayerModel.SetGoldWithoutNotify(gameModel.PlayerModel.Gold + gameModel.EnemyModel.Reward);

            if (LevelLogic.HasNextLevel())
                GameManager.Current.EnterState<InGameBattleRewardState>();
            else
            {
                Debug.Log("通关了");
                State<InGameEndingState>.Instance.EnterInfo.IsWin = true;
                GameManager.Current.EnterState<InGameEndingState>();
            }
        }
        
        private void OnPlayerLose()
        {
            Debug.Log("玩家失败");
            State<InGameEndingState>.Instance.EnterInfo.IsWin = false;
            GameManager.Current.EnterState<InGameEndingState>();
        }
        
        private void BattleSettlement(PlayerInteractionType interaction, int selectedCardIndex)
        {
            // 不清空动画
            // BattleAnimSystem.Instance.ClearAnimQueue();
            var battleModel = DataModel<BattleModel>.Instance;
            // 执行逻辑结算
            switch (interaction)
            {
                case PlayerInteractionType.DrawCard:
                    BattleLogic.DrawCardToHand();
                    break;
                case PlayerInteractionType.UseCard:
                    BattleLogic.UseCardFromHand(selectedCardIndex);
                    //处理Boss Buff
                    BuffLogic.TriggerBossBuffOnCardUse(battleModel);
                    break;
                case PlayerInteractionType.SacrificeCard:
                    BattleLogic.SacrificeCardFromHand(selectedCardIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interaction), interaction, null);
            }

            // 敌人攻击玩家一次
            BattleLogic.EnemyAttack();
            // 结算Buff
            BattleLogic.ApplyBuffs();
            // 处理回合内结算效果
            BattleLogic.TurnSettlement();
        }
        private async UniTask BattleAnim(CancellationToken token)
        {
            var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
            var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
            var handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
            
            // 播放全部动效
            try
            {
                while (await BattleAnimSystem.Instance.TryDequeAndPlayAnim(token)) {}
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Battle anim cancelled");
                return;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                // 移除所有等待卡牌
                handAreaView.RemoveAllPendingCards();
                // 在动画全部播放完后，统一删除待删除的卡牌
                BattleLogic.RemoveMarkedCards();
                // 移除所有无效Buff
                BattleLogic.RemoveUnavailableBuffs();
                // 刷新表现层
                playerView.RaiseAllPropertyChanged();
                enemyView.RaiseAllPropertyChanged();
                handAreaView.RaiseAllPropertyChanged();
            }
            OnBattleAnimFinished();
        }
    }
}