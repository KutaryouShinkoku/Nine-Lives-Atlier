using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.Common.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.InGame.Shop
{
    public class InGameUIShopSelectCardDeckView : UIRootView
    {
        public UnityEvent<int> onSelectCardDeck;
        public UnityEvent<int> onHoverCardDeck;
        public UnityEvent onCancel;
        
        [SerializeField]
        private CardCollectionView cardCollectionView;
        [SerializeField]
        private Button cancelButton;
        
        protected override void InitComponents()
        {
            base.InitComponents();
            cardCollectionView.onCardClick.AddListener((i, _) => onSelectCardDeck?.Invoke(i));
            cardCollectionView.onCardEnter.AddListener((i, _) => onHoverCardDeck?.Invoke(i));
            cancelButton.onClick.AddListener(() => onCancel?.Invoke());
        }
        
        protected override void OnDataModelChanged(DataModelBase dataModel)
        {
            base.OnDataModelChanged(dataModel);
            cardCollectionView.ItemSource = (dataModel as InGameModel)?.CardDeck;
        }
    }
}