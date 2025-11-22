using Game.Data;
using Game.Logic;
using Game.Model;
using Game.UI.InGame;
using UnityEngine.Events;
using WanFramework.Base;
using WanFramework.UI;
using IState = WanFramework.SM.IState;

namespace Game.SM.InGameState
{
    public class InGameEndingState : InGameStateBase
    {
        private UnityAction _onBack;
        private UnityAction _onInfinity;
        public struct StateEnterInfo
        {
            public bool IsWin { get; set; }
        }
        public StateEnterInfo EnterInfo;
        private void OnBack()
        {
            //结束删除存档
            SaveUtils.DeleteGame();
            GameManager.Current.EnterState<MainMenuState>();
        }
        private void OnInfinity()
        {
            LevelLogic.SetLevel(LevelIds.Level_Infinite_Entry);
            GameManager.Current.EnterState<InGameChooseLevelState>();
        }
        protected override void OnInit(GameManager machine)
        {
            base.OnInit(machine);
            _onBack = OnBack;
            _onInfinity = OnInfinity;
        }
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            var endingView = UISystem.Instance.ShowUI<UIEndingView>("InGame/UIEnding");
            endingView.onBackPressed.AddListener(_onBack);
            endingView.onInfinityPressed.AddListener(_onInfinity);
            endingView.SetResult(EnterInfo.IsWin);
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            var endingView = UISystem.Instance.GetUI<UIEndingView>("InGame/UIEnding");
            endingView.onBackPressed.RemoveListener(_onBack);
            endingView.onInfinityPressed.RemoveListener(_onInfinity);
            UISystem.Instance.Hide(endingView);
        }
    }
}