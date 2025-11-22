using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Data;
using Game.Localization;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.Tutorial;
using Game.UI.InGame;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState
{
    public sealed class InGameTutorialState : InGameStateBase
    {
        private UnityAction _onPlayerDrawCard;
        private UnityAction<int> _onPlayerUseCard;
        private UnityAction<int> _onPlayerSacrificeCard;
        
        private TutorialRunner _runner;
        [CanBeNull]
        private CancellationTokenSource _cts;

        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _runner = new TutorialRunner();
            _onPlayerDrawCard = _runner.OnPlayerDrawCard;
            _onPlayerUseCard = _runner.OnPlayerUseCard;
            _onPlayerSacrificeCard = _runner.OnPlayerSacrificeCard;
            _runner.OnFinished.AddListener(() => GameManager.Current.EnterState<MainMenuState>());
        }
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);

            //第一次进入教学阶段在SettingModel里面记录一下
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_Begin))
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_Begin);
            }

            var playerView = UISystem.Instance.ShowCommonView<InGamePlayerView>("InGame/Player");
            var enemyView = UISystem.Instance.ShowCommonView<InGameEnemyView>("InGame/Enemy");
            var handAreaView = UISystem.Instance.ShowCommonView<InGameHandAreaView>("InGame/HandAreaView");
            
            DataModel<BattleModel>.Instance.Reset();
            _cts = new CancellationTokenSource();
            _runner.Run(TutorialIds.Tutorial_Begin, _cts.Token);
            
            playerView.DataModel = DataModel<BattleModel>.Instance.PlayerModel;
            enemyView.DataModel = DataModel<BattleModel>.Instance.EnemyModel;
            handAreaView.DataModel = DataModel<BattleModel>.Instance;
            handAreaView.onDrawCard.AddListener(_onPlayerDrawCard);
            handAreaView.onUseCard.AddListener(_onPlayerUseCard);
            handAreaView.onSacrificeCard.AddListener(_onPlayerSacrificeCard);
            // 隐藏牌库
            SetCardDeckButtonVisible(false);
            SetDiscardCardButtonVisible(false);
            SetDrawCardButtonVisible(false);
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            base.OnExit(machine, nextState);
            var handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
            handAreaView.onDrawCard.RemoveListener(_onPlayerDrawCard);
            handAreaView.onUseCard.RemoveListener(_onPlayerUseCard);
            handAreaView.onSacrificeCard.RemoveListener(_onPlayerSacrificeCard);
            UISystem.Instance.Hide("InGame/HandAreaView");
            UISystem.Instance.Hide("InGame/Player");
            UISystem.Instance.Hide("InGame/Enemy");
            // 显示牌库
            SetCardDeckButtonVisible(true);
        }
    }
}