// //    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
// //    █▒▒░░░░░░░░░▒▒█
// //     █░░█░░░░░█░░█     Created by WanNeng
// //  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   02/08/2025 22:02
// // █░░█ ▀▄░░░░░░░▄▀ █░░█

using System;
using System.Buffers;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Data;
using Game.Logic;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.SM.InGameState.ShopState;
using Game.Tutorial;
using Game.UI.InGame;
using UnityEngine;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.SM;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.SM.InGameState.BattleState
{
    public sealed class InGameBattleRewardState : InGameBattleStateBase
    {
        private CancellationTokenSource _cts;
        private readonly TutorialRunner _runner = new();
        private CardModel[] _currentRewards;
        private int _selected = -1;
        private UnityAction<int> _onRewardSelected;
        private void OnRewardSelected(int selectedIndex) => _selected = selectedIndex;
        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onRewardSelected = OnRewardSelected;
        }
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            _cts = new CancellationTokenSource();
            var rewardView = UISystem.Instance.ShowUI<InGameUIBattleRewardView>("InGame/UIReward");
            rewardView.onSelectReward.AddListener(_onRewardSelected);
            EnsureCoverTopmost();
            DoRewardSelectionAsync(_cts.Token).Forget();
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Reward_Begin))
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_Reward_Begin);
                _runner.Run(TutorialIds.Tutorial_Reward_Begin, _cts.Token);
            }
        }
        private async UniTask DoRewardSelectionAsync(CancellationToken token)
        {
            var inGameModel = DataModel<InGameModel>.Instance;
            var rewardView = UISystem.Instance.GetUI<InGameUIBattleRewardView>("InGame/UIReward");
            int chooseCount = inGameModel.LevelInnerState == LevelInnerState.Boss ? 2 : 1;
            rewardView.SetCurReward(0, false);
            rewardView.SetMaxReward(chooseCount);
            try
            {
                //增加奖励数值
                var enemyModel = DataModel<BattleModel>.Instance.EnemyModel;
                inGameModel.Gold += enemyModel.Reward;
                // 直接执行三次
                for (int i = 0; i < chooseCount; i++)
                {
                    var isRare = inGameModel.LevelInnerState == LevelInnerState.Boss && i == 0;
                    var changeNumberTask = rewardView.PlaySetCurRewardAnim(i + 1, isRare, token);
                    var skipReward = isRare ? 5 : 2;
                    rewardView.SetSkipReward(skipReward);
                    _currentRewards = BattleLogic.GetRandomNewCard(3, isRare);
                    for (var j = 0; j < _currentRewards.Length; ++j)
                        rewardView.SetReward(j, _currentRewards[j]);
                    await rewardView.PlayShowRewardAnimAsync(token);
                    var selectedIndex = await WaitForRewardSelectionAsync(token);
                    var hideTask = rewardView.PlayHideRewardAnimAsync(token, selectedIndex);
                    await hideTask;
                    await changeNumberTask;
                    // 大于等于3为跳过奖励
                    if (selectedIndex >= 3) DataModel<InGameModel>.Instance.Gold += skipReward;
                    else DataModel<InGameModel>.Instance.CardDeck.Add(_currentRewards[selectedIndex]);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Cancelled reward selection");
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            // 奖励选完进入关卡选择
            LevelLogic.TryGotoNextLevel();
            // 保存游戏
            // SaveUtil里自动注册，监听当关卡发生变化时自动保存？
            SaveUtils.SaveGame();
            GameManager.Current.EnterState<InGameChooseLevelState>();
        }
        private async UniTask<int> WaitForRewardSelectionAsync(CancellationToken token)
        {
            _selected = -1;
            while (_selected == -1) await UniTask.NextFrame(token);
            return _selected;
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            var rewardView = UISystem.Instance.GetUI<InGameUIBattleRewardView>("InGame/UIReward");
            rewardView.onSelectReward.AddListener(_onRewardSelected);
            UISystem.Instance.Hide("InGame/UIReward");
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}