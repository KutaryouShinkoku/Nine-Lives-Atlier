using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using WanFramework.UI.DataComponent;

namespace Game.Logic
{
    public static class SetupLogic
    {
        public static void SetCharacter(CharacterIds characterId)
        {
            var m = DataModel<InGameModel>.Instance;
            m.CharacterId = characterId;
            m.CardDeck.Clear();
            var characterData = characterId.Data();
            foreach (var cardId in characterData.InitialCards)
                DataModel<InGameModel>.Instance.CardDeck.Add(new CardModel
                {
                    Id = cardId,
                    AdditionEffectVal1 = 0,
                    AdditionEffectVal2 = 0,
                    AdditionEffectVal3 = 0,
                    FeeChange = new CardCost
                    {
                        Air = 0,
                        Earth = 0,
                        Fire = 0,
                        Water = 0,
                    }
                });
        }
    }
}