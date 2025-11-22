using System;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.Common.Components;
using UnityEngine;
using UnityEngine.UI;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.UI.Common
{
    public enum CardDeckViewType
    {
        HandCards, DiscardCards, RemainCards, CardDeck
    }
    public class UICardDeckView : UIRootView
    {
        [SerializeField]
        private CardCollectionView cardCollectionView;
        [SerializeField]
        private Button buttonClose;

        protected override void InitComponents()
        {
            base.InitComponents();
            buttonClose.onClick.AddListener(() => UISystem.Instance.Hide(this));
        }
        private DataCollection<CardModel> GetCardCollection(CardDeckViewType cardDeckViewType) => cardDeckViewType switch
        {
            CardDeckViewType.HandCards => DataModel<BattleModel>.Instance.HandCards,
            CardDeckViewType.DiscardCards => DataModel<BattleModel>.Instance.DiscardCards,
            CardDeckViewType.RemainCards => DataModel<BattleModel>.Instance.RemainCards,
            CardDeckViewType.CardDeck => DataModel<InGameModel>.Instance.CardDeck,
            _ => throw new ArgumentOutOfRangeException(nameof(cardDeckViewType), cardDeckViewType, null)
        };

        public void SetCardDeckViewType(CardDeckViewType cardDeckViewType) => cardCollectionView.ItemSource = GetCardCollection(cardDeckViewType);
    }
}