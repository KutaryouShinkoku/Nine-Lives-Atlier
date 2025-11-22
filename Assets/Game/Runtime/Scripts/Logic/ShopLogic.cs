using System;
using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.InGame.Shop;
using UnityEngine;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using Random = UnityEngine.Random;

namespace Game.Logic
{
    public static class ShopCostConfig
    {
        // 初始费用
        public static int InitialRefreshCost = 2;
        public static int InitialDeleteCost = 5;
        public static int InitialUpgradeCost = 2;

        // 每次操作递增量
        public static int RefreshCostIncrement = 1;
        public static int DeleteCostIncrement = 5;
        public static int UpgradeCostIncrement = 4;
        
        // 全局系数
        public static float GlobalValueFactor = 1.2f;
    }

    public static class ShopLogic
    {
        #region 商店操作基础逻辑
        private static int GetRandomCardBaseCost(int price)
        {
            return GetIncreasedCost(Random.value switch
            {
                < 0.2f => Mathf.Max(1, price - 1),
                > 0.8f => price + 1,
                _ => price
            });
        }
        private static int GetIncreasedCost(int price)
        {
            var m = DataModel<InGameModel>.Instance;
            return Mathf.RoundToInt(price * Mathf.Pow(ShopCostConfig.GlobalValueFactor, m.PassedLevelCount));
        }
        public static int GetNextRefreshCost()
            => DataModel<ShopModel>.Instance.RefreshCost + GetIncreasedCost(ShopCostConfig.RefreshCostIncrement);
        public static int GetNextDeleteCost()
            => DataModel<ShopModel>.Instance.DeleteCost + GetIncreasedCost(ShopCostConfig.DeleteCostIncrement);
        public static int GetNextUpgradeCost(int delta)
            => DataModel<ShopModel>.Instance.UpgradeCost + GetIncreasedCost(ShopCostConfig.UpgradeCostIncrement) * delta;
        private static int GetRandomAdditionValue(int val)
        {
            var m = DataModel<InGameModel>.Instance;
            var rnd = Random.value;
            if (rnd > 0.2f) return 0;
            var min = Mathf.FloorToInt(Mathf.Max(-val, -val * 0.5f));
            var max = Mathf.CeilToInt(val * 0.5f);
            var rndVal = Random.Range(min, max);
            if (rndVal >= 0) rndVal += 1;
            return rndVal;
        }
        /// <summary>
        /// 初始化商店模型
        /// </summary>
        public static void InitializeShopModel()
        {
            var shopView = UISystem.Instance.GetUI<InGameUIShopView>("InGame/Shop/UIShop");

            var shopModel = DataModel<ShopModel>.Instance;
            shopModel.RefreshCost = GetIncreasedCost(ShopCostConfig.InitialRefreshCost);
            shopModel.DeleteCost = GetIncreasedCost(ShopCostConfig.InitialDeleteCost);
            shopModel.UpgradeCost = GetIncreasedCost(ShopCostConfig.InitialUpgradeCost);

            for (var i = 0; i < shopView.CardCount; ++i)
                shopView.SetCardInteractable(i, true);
            // 生成ShopModel内的卡牌

            for (var i = 0; i < shopModel.Cards.Length; i++)
            {
                if (shopModel.Cards[i].HasBuy) continue;

                var newCard = BattleLogic.GetRandomNewCard(1, isRareOnly: false, includeShop: true)[0];
                var cardData = newCard.Id.Data();
                newCard.AdditionEffectVal1 = GetRandomAdditionValue(cardData.EffectValue[0]);
                newCard.AdditionEffectVal2 = GetRandomAdditionValue(cardData.EffectValue[1]);
                newCard.AdditionEffectVal3 = GetRandomAdditionValue(cardData.EffectValue[2]);
                shopModel.Cards[i].Card.Id = newCard.Id;
                shopModel.Cards[i].Card.AdditionEffectVal1 = newCard.AdditionEffectVal1;
                shopModel.Cards[i].Card.AdditionEffectVal2 = newCard.AdditionEffectVal2;
                shopModel.Cards[i].Card.AdditionEffectVal3 = newCard.AdditionEffectVal3;
                shopModel.Cards[i].Cost = GetRandomCardBaseCost(newCard.Id.Data().Value);
            }
        }

        /// <summary>
        /// 刷新商店卡牌
        /// </summary>
        public static bool TryRefreshShop(out int cost)
        {
            var shopView = UISystem.Instance.GetUI<InGameUIShopView>("InGame/Shop/UIShop");
            var shopModel = DataModel<ShopModel>.Instance;
            var inGameModel = DataModel<InGameModel>.Instance;


            cost = shopModel.RefreshCost;
            if (inGameModel.Gold < cost)
                return false;

            inGameModel.Gold -= cost;
            shopModel.RefreshCost = GetNextRefreshCost();

            for (var i = 0; i < shopView.CardCount; ++i)
                shopView.SetCardInteractable(i, true);
            // 生成ShopModel内的卡牌

            for (var i = 0; i < shopModel.Cards.Length; i++)
            {
                if (shopModel.Cards[i].HasBuy) continue;

                var newCard = BattleLogic.GetRandomNewCard(1,isRareOnly: false,includeShop: true)[0];
                var cardData = newCard.Id.Data();
                newCard.AdditionEffectVal1 = GetRandomAdditionValue(cardData.EffectValue[0]);
                newCard.AdditionEffectVal2 = GetRandomAdditionValue(cardData.EffectValue[1]);
                newCard.AdditionEffectVal3 = GetRandomAdditionValue(cardData.EffectValue[2]);
                shopModel.Cards[i].Card.Id = newCard.Id;
                shopModel.Cards[i].Card.AdditionEffectVal1 = newCard.AdditionEffectVal1;
                shopModel.Cards[i].Card.AdditionEffectVal2 = newCard.AdditionEffectVal2;
                shopModel.Cards[i].Card.AdditionEffectVal3 = newCard.AdditionEffectVal3;
                shopModel.Cards[i].Cost = GetRandomCardBaseCost(newCard.Id.Data().Value);
            }

            shopView.PlayAllCardNormalAnim();

            return true;
        }

        /// <summary>
        /// 删除卡牌操作
        /// </summary>
        public static bool TryDeleteCard(int cardDeckIndex, out int cost)
        {
            var shopModel = DataModel<ShopModel>.Instance;
            var inGameModel = DataModel<InGameModel>.Instance;

            cost = shopModel.DeleteCost;
            if (inGameModel.Gold < cost)
                return false;
            if (cardDeckIndex < 0 || cardDeckIndex >= inGameModel.CardDeck.Count)
                return false;

            inGameModel.Gold -= cost;
            inGameModel.CardDeck.RemoveAt(cardDeckIndex);
            shopModel.DeleteCost = GetNextDeleteCost();
            return true;
        }

        #endregion

        #region 卡牌升级相关逻辑

        /// <summary>
        /// 计算单个属性变化的费用（等差数列）
        /// </summary>
        private static int CalculateSingleEffectCost(int current, int target, int baseCost)
        {
            int delta = Mathf.Abs(target - current);
            if (delta == 0) return 0;
            return delta * (2 * baseCost + (delta - 1) * 2) / 2;
        }

        /// <summary>
        /// 执行卡牌升级
        /// </summary>
        public static bool TryUpgradeCard(int cardDeckIndex, int[] effectValues, out int cost)
        {
            var shopModel = DataModel<ShopModel>.Instance;
            var inGameModel = DataModel<InGameModel>.Instance;

            var card = inGameModel.CardDeck[cardDeckIndex];
            int delta = CalculateTotalDelta(card, effectValues);
            cost = GetIncreasedCost(CalculateUpgradeCost(card, effectValues, shopModel.UpgradeCost));

            if (inGameModel.Gold < cost)
                return false;

            inGameModel.Gold -= cost;
            ApplyUpgrade(card, effectValues);
            shopModel.UpgradeCost = GetNextUpgradeCost(delta);
            return true;
        }
        private static int CalculateTotalDelta(CardModel card, int[] targetValues)
        {
            return Mathf.Abs(card.AdditionEffectVal1 - targetValues[0]) +
                   Mathf.Abs(card.AdditionEffectVal2 - targetValues[1]) +
                   Mathf.Abs(card.AdditionEffectVal3 - targetValues[2]);
        }

        private static void ApplyUpgrade(CardModel card, int[] effectValues)
        {
            card.AdditionEffectVal1 = effectValues[0];
            card.AdditionEffectVal2 = effectValues[1];
            card.AdditionEffectVal3 = effectValues[2];
        }

        private static int CalculateUpgradeCost(CardModel card, int[] targetValues, int baseCost)
        {
            return CalculateSingleEffectCost(card.AdditionEffectVal1, targetValues[0], baseCost) +
                   CalculateSingleEffectCost(card.AdditionEffectVal2, targetValues[1], baseCost) +
                   CalculateSingleEffectCost(card.AdditionEffectVal3, targetValues[2], baseCost);
        }

        /// <summary>
        /// 计算升级费用和总变化点数
        /// </summary>
        public static (int cost, int totalDelta) CalculateUpgradeCostWithDelta(CardModel card,int[] targetValues,int currentActionCost)
        {
            // 计算每个属性的变化量
            int totalDelta = CalculateTotalDelta(card, targetValues);

            // 计算总费用
            int cost = CalculateUpgradeCost(card, targetValues, currentActionCost);

            return (cost, totalDelta);
        }

        #endregion
    }
}