using Game.Model.InGameSubModel;
using WanFramework.UI.DataComponent;

namespace Game.Model
{
    public partial class ShopCardModel : DataModelBase
    {
        [AutoNotify]
        private CardModel _card = new();
        [AutoNotify]
        private int _cost;
        [AutoNotify]
        private bool _hasBuy;
    }
    [ShowInDataModelEditor]
    public partial class ShopModel : DataModelBase
    {
        public ShopCardModel[] Cards { get; } = CreateModelArray<ShopCardModel>(6);

        [AutoNotify]
        private int _refreshCost;

        [AutoNotify]
        private int _deleteCost;

        [AutoNotify]
        private int _upgradeCost;
    }
}