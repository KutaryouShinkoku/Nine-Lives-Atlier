using System.Threading;
using Game.Data;
using Game.Model;
using Game.Tutorial;
using Game.UI.MainMenu;
using WanFramework.Base;
using WanFramework.SM;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.SM
{
    public class MainMenuState : GameState
    {
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            UISystem.Instance.ShowUI<UIMainMenuView>("MainMenu/UIMainMenu");
        }

        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            UISystem.Instance.Hide("MainMenu/UIMainMenu");
        }
    }
}