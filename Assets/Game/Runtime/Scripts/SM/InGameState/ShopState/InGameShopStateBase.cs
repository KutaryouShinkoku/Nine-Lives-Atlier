using Game.UI.InGame.Shop;
using WanFramework.Base;
using WanFramework.SM;
using WanFramework.UI;

namespace Game.SM.InGameState.ShopState
{
    public abstract class InGameShopStateBase : InGameStateBase
    {
        protected override void OnEnter(GameManager machine, IState previousState)
        {
            base.OnEnter(machine, previousState);
            UISystem.Instance.ShowUI("InGame/Shop/UIShop");
        }
        protected override void OnExit(GameManager machine, IState nextState)
        {
            if (nextState is InGameShopStateBase) return;
            base.OnExit(machine, nextState);
            UISystem.Instance.Hide("InGame/Shop/UIShop");
            UISystem.Instance.Hide("InGame/Shop/UIShopDeleteCard");
            UISystem.Instance.Hide("InGame/Shop/UIShopSelectCardDeck");
            UISystem.Instance.Hide("InGame/Shop/UIShopUpgradeCard");
        }
    }
}