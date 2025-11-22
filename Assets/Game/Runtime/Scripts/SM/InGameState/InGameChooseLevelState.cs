using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Logic;
using Game.Model;
using Game.SM.InGameState.BattleState;
using Game.SM.InGameState.ShopState;
using Game.Tutorial;
using Game.UI.InGame;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState
{
    /// <summary>
    /// 选关阶段
    /// </summary>
    public sealed class InGameChooseLevelState : InGameStateBase
    {
        private UnityAction _onChooseLevel;

        //跑教程的
        private readonly TutorialRunner _runner = new();
        private CancellationTokenSource _cts;

        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onChooseLevel = OnChooseLevel;
        }
        private void OnChooseLevel()
        {
            var levelState = DataModel<InGameModel>.Instance.LevelInnerState;
            var ui = UISystem.Instance.GetUI<InGameUIChooseLevelView>("InGame/UIChooseLevel");

            ui.PlayHide(_cts.Token)
              .ContinueWith(() =>
              {
                  // 动画结束后再切
                  if (levelState == LevelInnerState.Battle)
                  {
                      BattleLogic.PrepareBattle();
                      GameManager.Current.EnterState<InGameBattleState>();
                  }
                  else if (levelState == LevelInnerState.Shop)
                  {
                      ShopLogic.InitializeShopModel();
                      GameManager.Current.EnterState<InGameShopState>();
                  }
                  else if (levelState == LevelInnerState.Boss)
                  {
                      Debug.Log("Boss!!!");
                      BattleLogic.PrepareBattle();
                      GameManager.Current.EnterState<InGameBattleState>();
                  }
                  else Debug.Log($"What! You are in state {levelState}!");
              })
              .Forget();
        }
        
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            _cts = new();
            var ui = UISystem.Instance.ShowUI<InGameUIChooseLevelView>("InGame/UIChooseLevel");
            ui.onSelectLevel.AddListener(_onChooseLevel);
            ui.DataModel = DataModel<InGameModel>.Instance;
            ui.PlayShow(_cts.Token);
            EnsureCoverTopmost();

            //第一次进入地图界面执行
            if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_EnterMap_Begin))
            {
                DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_EnterMap_Begin);
                _runner.Run(TutorialIds.Tutorial_EnterMap_Begin, _cts.Token);
            }
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            var ui = UISystem.Instance.GetUI<InGameUIChooseLevelView>("InGame/UIChooseLevel");
            ui.onSelectLevel.RemoveListener(_onChooseLevel);
            UISystem.Instance.Hide(ui);
        }
    }
}