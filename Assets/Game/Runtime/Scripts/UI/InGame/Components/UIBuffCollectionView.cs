using Game.Data;
using Game.UI.Common.Components;
using WanFramework.UI;
using Game.Model.InGameSubModel;

namespace Game.UI.InGame.Components
{
    public class UIBuffCollectionView : CollectionView<BuffModel>
    {
        protected override void OnElementAdding(SubView subView, BuffModel newElement, int newIndex)
        {
            base.OnElementAdding(subView, newElement, newIndex);
            subView.DataModel = newElement;
        }
    }
}