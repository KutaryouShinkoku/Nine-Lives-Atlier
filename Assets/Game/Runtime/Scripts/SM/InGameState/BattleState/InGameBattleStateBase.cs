// //    ▄▀▀▀▄▄▄▄▄▄▄▀▀▀▄
// //    █▒▒░░░░░░░░░▒▒█
// //     █░░█░░░░░█░░█     Created by WanNeng
// //  ▄▄  █░░░▀█▀░░░█  ▄▄  Created   02/08/2025 22:02
// // █░░█ ▀▄░░░░░░░▄▀ █░░█

using Game.Model;
using Game.UI.InGame;
using WanFramework.Base;
using WanFramework.SM;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.SM.InGameState.BattleState
{
    /// <summary>
    /// 负责管理战斗场景，包括玩家和敌人视图
    /// </summary>
    public abstract class InGameBattleStateBase : InGameStateBase
    {
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            var playerView = UISystem.Instance.IsShowing("InGame/Player") ? 
                UISystem.Instance.GetCommonView("InGame/Player") :
                UISystem.Instance.ShowCommonView("InGame/Player");
            playerView.DataModel = DataModel<BattleModel>.Instance.PlayerModel;
            var enemyView = UISystem.Instance.IsShowing("InGame/Enemy") ? 
                UISystem.Instance.GetCommonView("InGame/Enemy") :
                UISystem.Instance.ShowCommonView("InGame/Enemy");
            enemyView.DataModel = DataModel<BattleModel>.Instance.EnemyModel;
        }
        
        protected override void OnExit(GameManager machine, IState nextState)
        {
            base.OnExit(machine, nextState);
            if (nextState is not InGameBattleStateBase)
            {
                UISystem.Instance.Hide("InGame/Player");
                UISystem.Instance.Hide("InGame/Enemy");
            }
        }
    }
}