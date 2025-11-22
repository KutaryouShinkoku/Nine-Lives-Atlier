using Game.Data;
using Game.Logic;
using Game.Model;
using Game.Tutorial;
using Game.UI.InGame;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState
{
    /// <summary>
    /// 选角色阶段
    /// </summary>
    public sealed class InGameChooseCharacterState : InGameStateBase
    {
        private UnityAction<CharacterIds> _onChooseCharacter;

        //跑教程的
        private readonly TutorialRunner _runner = new();
        private CancellationTokenSource _cts;

        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onChooseCharacter = OnChooseCharacter;
        }
        
        private void OnChooseCharacter(CharacterIds character)
        {
            UISystem.Instance.Hide("Common/UICardDeck");
            // 播放完成隐藏动画后，进入下一阶段
            var ui = UISystem.Instance.GetUI<InGameUIChooseCharacter>("InGame/UIChooseCharacter");
            ui.CanInteract = false;
            ui.PlayHide(_cts.Token)
                .ContinueWith(() =>
                {
                    SetupLogic.SetCharacter(character);
                    // 暂时写死用Map1
                    LevelLogic.SetLevel(LevelIds.Map1_Entry);
                    // 选择完成角色后保存一次
                    SaveUtils.SaveGame();
                    // 进入选关阶段
                    GameManager.Current.EnterState<InGameChooseLevelState>();
                })
                .Forget();
        }

        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            _cts = new();
            var ui = UISystem.Instance.ShowUI<InGameUIChooseCharacter>("InGame/UIChooseCharacter");
            ui.CanInteract = false;
            ui.onConfirmCharacter.AddListener(_onChooseCharacter);
            ui.DataModel = DataModel<InGameModel>.Instance;
            EnsureCoverTopmost();
            ui.PlayShow(_cts.Token).ContinueWith(async () =>
            {
                if (_cts.IsCancellationRequested) return;
                //第一次进入角色选择界面执行
                if (!DataModel<SettingModel>.Instance.TutorialIds.Contains(TutorialIds.Tutorial_EnterCharaSelection_Begin))
                {
                    DataModel<SettingModel>.Instance.TutorialIds.Add(TutorialIds.Tutorial_EnterCharaSelection_Begin);
                    await _runner.RunAsync(TutorialIds.Tutorial_EnterCharaSelection_Begin, _cts.Token);
                }
                ui.CanInteract = true;
            }).Forget();
            // 隐藏牌库
            SetCardDeckButtonVisible(false);
        }

        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            UISystem.Instance.Hide("Common/UICardDeck");
            var ui = UISystem.Instance.GetUI<InGameUIChooseCharacter>("InGame/UIChooseCharacter");
            ui.onConfirmCharacter.RemoveListener(_onChooseCharacter);
            UISystem.Instance.Hide(ui);
            // 显示牌库
            SetCardDeckButtonVisible(true);
        }
    }
}