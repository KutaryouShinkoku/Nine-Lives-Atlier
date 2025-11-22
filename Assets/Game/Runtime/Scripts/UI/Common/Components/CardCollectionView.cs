using Game.Model.InGameSubModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.Common.Components
{
    public class CardCollectionView : CollectionView<CardModel>
    {
        public UnityEvent<int, DataCollection<CardModel>> onCardClick;
        public UnityEvent<int, DataCollection<CardModel>> onCardEnter;
        private UnityAction<SelectableUICardView> _cardClickListener;
        private UnityAction<SelectableUICardView> _cardEnterListener;
        protected override void InitComponents()
        {
            base.InitComponents();
            _cardClickListener = card => onCardClick?.Invoke(ItemSource.IndexOf(card.DataModel as CardModel), ItemSource);
            _cardEnterListener = card => onCardEnter?.Invoke(ItemSource.IndexOf(card.DataModel as CardModel), ItemSource);
        }
        protected override void OnElementAdding(SubView subView, CardModel newElement, int newIndex)
        {
            base.OnElementAdding(subView, newElement, newIndex);
            subView.DataModel = newElement;
            //subView.GetComponent<Canvas>().overrideSorting = false;
            if (subView is SelectableUICardView selectableUICardView)
            {
                selectableUICardView.SetRootDataModel(newElement);
                selectableUICardView.onCardClick.AddListener(_cardClickListener);
                selectableUICardView.onCardEnter.AddListener(_cardEnterListener);
            }
        }
        protected override void OnElementRemoving(SubView subView, int oldIndex)
        {
            base.OnElementRemoving(subView, oldIndex);
            if (subView is SelectableUICardView selectableUICardView)
            {
                selectableUICardView.onCardClick.RemoveListener(_cardClickListener);
                selectableUICardView.onCardEnter.RemoveListener(_cardEnterListener);
            }
            subView.DataModel = null;
        }
    }
}