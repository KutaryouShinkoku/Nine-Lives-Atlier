using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.SM.InGameState;
using System;
using Game.BattleAnim;
using UnityEngine;
using WanFramework.SM;
using WanFramework.UI.DataComponent;
using Game.UI.InGame;
using WanFramework.UI;
using System.Collections.Generic;
using WanFramework.Data;
using System.Linq;
using UnityEngine.TextCore.Text;
using Game.UI.Common;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Threading;

namespace Game.Logic
{
    public static class BattleLogic
    {
        /// <summary>
        /// 卡牌使用的场景，用于条件判断
        /// </summary>
        public enum CardUseContext
        {
            Use,
            Sacrifice,
            Discard
        }

        #region Helper Methods for Cost Calculation

        /// <summary>
        /// 将 int 数组转换为 CardCost 类型，数组顺序为 [Fire, Water, Earth, Air]
        /// </summary>
        public static CardCost ConvertCost(int[] costArray)
        {
            // 如果数组为 null，则返回全 0 的费用
            if (costArray == null || costArray.Length < 4)
            {
                return new CardCost { Fire = 0, Water = 0, Earth = 0, Air = 0 };
            }

            int fire = costArray.Length > 0 ? costArray[0] : 0;
            int water = costArray.Length > 1 ? costArray[1] : 0;
            int earth = costArray.Length > 2 ? costArray[2] : 0;
            int air = costArray.Length > 3 ? costArray[3] : 0;

            return new CardCost
            {
                Fire = fire,
                Water = water,
                Earth = earth,
                Air = air
            };
        }

        /// <summary>
        /// 判断玩家当前可用资源是否足够支付卡牌成本
        /// </summary>
        private static bool HasEnoughResource(CardCost available, CardCost cost)
        {
            return available.Fire >= cost.Fire &&
                   available.Water >= cost.Water &&
                   available.Earth >= cost.Earth &&
                   available.Air >= cost.Air;
        }

        /// <summary>
        /// 判断玩家当前可用资源是否足够支付某张卡牌（费用为卡牌基础费用加上卡牌的费用变化值）
        /// </summary>
        public static bool HasEnoughResource(CardCost available, CardModel cardModel)
        {
            var baseCost = ConvertCost(cardModel.Id.Data().Cost);
            var totalCost = baseCost + cardModel.FeeChange;
            return HasEnoughResource(available, totalCost);
        }

        #endregion

        #region 卡牌删除与弃牌相关

        /// <summary>
        /// 全局静态待删除列表，用于延迟删除手牌中的卡牌索引
        /// </summary>
        private static readonly List<int> _pendingRemovalIndices = new List<int>();

        /// <summary>
        /// 标记待删除的卡牌索引，而不立即删除 DataCollection 中的对应数据项。
        /// </summary>
        public static void MarkCardForRemoval(int handCardIndex)
        {
            if (!_pendingRemovalIndices.Contains(handCardIndex))
            {
                BattleAnimSystem.Instance.QueueDiscardCardAnim(handCardIndex);
                _pendingRemovalIndices.Add(handCardIndex);
            }
        }

        /// <summary>
        /// 删除所有标记的卡牌，将其从 HandCards 移入 DiscardCards。建议在所有动画播放结束后调用此方法，
        /// 以确保表现层动画完成后再清除数据。
        /// </summary>
        public static void RemoveMarkedCards()
        {
            if (_pendingRemovalIndices.Count == 0)
                return;

            _pendingRemovalIndices.Sort();
            var battleModel = DataModel<BattleModel>.Instance;
            for (int i = _pendingRemovalIndices.Count - 1; i >= 0; i--)
            {
                int index = _pendingRemovalIndices[i];
                if (index >= 0 && index < battleModel.HandCards.Count)
                {
                    // 将该卡牌加入弃牌区
                    var card = battleModel.HandCards[index];
                    battleModel.DiscardCards.Add(card);
                    battleModel.HandCards.RemoveAt(index);
                }
            }
            _pendingRemovalIndices.Clear();
        }

        #endregion

        private static void PrepareBuff(BuffPoolTable.Entry pool, int count, DataCollection<BuffModel> outList)
        {
            var buffCacheList = ListPool<int>.Get();
            try
            {
                Algorithm.Roulette(pool.Weights.AsSpan(), pool.Buffs.Length, count, buffCacheList);
                foreach (var index in buffCacheList)
                    outList.Add(new BuffModel()
                    {
                        Id = pool.Buffs[index],
                        Stack = pool.Stacks[index],
                    });
            }
            finally
            {
                ListPool<int>.Release(buffCacheList);
            }
        }
        
        public static bool IsPendingRemove(int id) => _pendingRemovalIndices.Contains(id);
        public static void PrepareBattle()
        {
            var model = DataModel<InGameModel>.Instance;
            var battleModel = DataModel<BattleModel>.Instance;

            // 如果 NextLevel 已设置，就先切到那一关
            if (model.NextLevel != default(LevelIds) && model.NextLevel != LevelIds.Unknown)
            {
                model.CurrentLevel = model.NextLevel;
                model.NextLevel = LevelIds.Unknown;
            }

            var level = model.CurrentLevel.Data();
            battleModel.Reset();

            // 选择敌人
            EnemyIds enemyId;
            if (model.LevelInnerState == LevelInnerState.Boss
                && model.NextBoss != default(EnemyIds)
                && model.NextBoss != EnemyIds.Unknown)
            {
                // 只在 Boss 阶段使用预先缓存的下一关 Boss
                enemyId = model.NextBoss;
                model.NextBoss = EnemyIds.Unknown;
            }
            else
            {
                // 其它阶段或没有缓存，就随机选普通池里的敌人
                var pool = (model.LevelInnerState == LevelInnerState.Boss
                            ? level.BossEnemyPool.Data()
                            : level.NormalBattleEnemyPool.Data());
                enemyId = Algorithm.Roulette(pool.Enemies, pool.Weights);
            }

            var enemyData = enemyId.Data();
            battleModel.EnemyModel.EnemyId = enemyId;
            battleModel.EnemyModel.SetAttackWithoutNotify(Mathf.RoundToInt(enemyData.EnemyAttributes[0] * model.AttackScale));
            battleModel.EnemyModel.SetHealthWithoutNotify(Mathf.RoundToInt(enemyData.EnemyAttributes[1] * model.HealthScale));
            battleModel.EnemyModel.SetRewardWithoutNotify(enemyData.EnemyAttributes[2]);
            // 初始化角色
            battleModel.PlayerModel.SetCharacterIdWithoutNotify(model.CharacterId);
            // 初始化生命值
            battleModel.PlayerModel.SetHealthWithoutNotify(99);
            // 初始化资源
            battleModel.PlayerModel.SetResourcesWithoutNotify(new CardCost { Fire = 0, Water = 0, Earth = 0, Air = 0 });
            battleModel.PlayerModel.IsFocusCondition = false;
            // 初始化手牌、弃牌、牌库
            battleModel.HandCards.Clear();
            battleModel.DiscardCards.Clear();
            battleModel.RemainCards.Clear();
            foreach (var cardModel in model.CardDeck)
                battleModel.RemainCards.Add(cardModel);
            Shuffle(battleModel.RemainCards);
            // 初始化敌人和玩家的Buff
            if (model.LevelInnerState == LevelInnerState.Boss)
            {
                PrepareBuff(level.BossBuffPool.Data(), level.BossBuffCount, battleModel.EnemyModel.enemyBuffs);
                PrepareBuff(level.PlayerBuffPool.Data(), level.PlayerBuffCount, battleModel.PlayerModel.Buffs);
            }
            //初始抽卡
            InitialDrawCard();
        }

        public static void InitialDrawCard()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            var cardCount = battleModel.PlayerModel.CharacterId.Data().InitialDraws;
            for (int i = 0; i < cardCount; i++)
            {
                var drawnCard = battleModel.RemainCards[0];
                battleModel.RemainCards.RemoveAt(0);
                battleModel.HandCards.Add(drawnCard);
            }
        }

        /// <summary>
        /// 将牌从A洗入B，并清空A
        /// </summary>
        /// <param name="from">从哪</param>
        /// <param name="to">洗到哪</param>
        private static void ShuffleTo(DataCollection<CardModel> from, DataCollection<CardModel> to)
        {
            foreach (var cardModel in from)
                to.Add(cardModel);
            from.Clear();
            Shuffle(to);
        }

        /// <summary>
        /// 洗牌算法
        /// </summary>
        /// <param name="cards"></param>
        private static void Shuffle(DataCollection<CardModel> cards)
        {
            for (var i = cards.Count - 1; i >= 0; --i)
            {
                var selection = UnityEngine.Random.Range(0, i + 1);
                (cards[i], cards[selection]) = (cards[selection], cards[i]);
            }
        }

        public static bool CanDrawCard()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            return (battleModel.HandCards.Count - _pendingRemovalIndices.Count) < battleModel.EffectiveMaxHandCards;
        }
        /// <summary>
        /// 抽牌事件  
        /// 若手牌达到上限，则无法抽牌。
        /// </summary>
        public static void DrawCardToHand()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            if (!CanDrawCard()) return;
            // 如果剩余牌堆为空，则将弃牌堆洗牌回剩余牌堆
            if (battleModel.RemainCards.Count == 0)
            {
                if (battleModel.DiscardCards.Count > 0)
                {
                    ShuffleTo(battleModel.DiscardCards, battleModel.RemainCards);
                }
                else
                {
                    Debug.Log("牌库为空，无法抽牌。");
                    return;
                }
            }
            Debug.Log("抽牌事件：抽取一张卡牌");
            // 从 RemainCards 中取出第一张
            var drawnCard = battleModel.RemainCards[0];
            battleModel.RemainCards.RemoveAt(0);
            battleModel.HandCards.Add(drawnCard);
            
            //清除乘风
            battleModel.PlayerModel.Tailwind = 0;
            BattleAnimSystem.Instance.PlayGaugesValueChangeAnim(0,0.1f);
            
            // 添加动画队列，参数为新加入的卡牌索引
            BattleAnimSystem.Instance.QueueDrawCardToHandAnim(battleModel.HandCards.Count - 1);
        }

        /// <summary>
        /// 使用卡牌事件（花费费用）  
        /// 执行前会检查玩家资源是否足够，若足够则扣除费用并播放对应动画。
        /// </summary>
        public static void UseCardFromHand(int handCardIndex)
        {
            BattleAnimSystem.Instance.QueueMoveCardToEnemyAnim(handCardIndex);
            var battleModel = DataModel<BattleModel>.Instance;
            if (handCardIndex < 0 || handCardIndex >= battleModel.HandCards.Count)
            {
                Debug.Log("使用卡牌事件：无效的卡牌索引");
                return;
            }
            var cardModel = battleModel.HandCards[handCardIndex];
            var cardData = cardModel.Id.Data();
            // 费用 = 卡牌表费用 + cardModel.FeeChange
            CardCost baseCost = ConvertCost(cardData.Cost);
            CardCost totalCost = baseCost + cardModel.FeeChange;

            if (!HasEnoughResource(battleModel.PlayerModel.Resources, cardModel))
            {
                Debug.Log("使用卡牌事件：资源不足，无法使用该卡牌");

                return;
            }
            // 先标记删除，再处理卡牌效果
            MarkCardForRemoval(handCardIndex);

            BattleAnimSystem.Instance.QueueUseCardFromHandAnim(handCardIndex);

            // 扣除费用
            BattleAnimSystem.Instance.QueueResourceBarValChange(battleModel.PlayerModel.Resources - totalCost, 0.2f);
            battleModel.PlayerModel.SetResourcesWithoutNotify(battleModel.PlayerModel.Resources - totalCost);
            Debug.Log($"使用卡牌：扣除费用 => {totalCost.Fire}/{totalCost.Water}/{totalCost.Earth}/{totalCost.Air}");
            
            // 乘风
            battleModel.PlayerModel.Tailwind++;
            BattleAnimSystem.Instance.PlayGaugesValueChangeAnim(battleModel.PlayerModel.Tailwind,0.1f);
            Debug.Log($"乘风中，当前为{battleModel.PlayerModel.Tailwind}");

            // 结算卡牌效果(场景=Use)
            ProcessCardEffects(battleModel, cardModel, cardData, CardUseContext.Use);
            BattleAnimSystem.Instance.QueueSetCardVisibility(handCardIndex, false);

        }

        /// <summary>
        /// 炼制（献祭）卡牌事件  
        /// 将卡牌转化为资源，并对敌人施加属性加成，同时播放炼制动画。
        /// </summary>
        public static void SacrificeCardFromHand(int handCardIndex)
        {
            var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
            var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");

            BattleAnimSystem.Instance.QueueMoveCardToResourceAnim(handCardIndex);
            var battleModel = DataModel<BattleModel>.Instance;
            if (handCardIndex < 0 || handCardIndex >= battleModel.HandCards.Count)
            {
                Debug.Log("炼制卡牌事件：无效的卡牌索引");
                return;
            }
            var cardModel = battleModel.HandCards[handCardIndex];
            var cardData = cardModel.Id.Data();
            // 费用 = 卡牌表费用 + cardModel.FeeChange
            CardCost baseCost = ConvertCost(cardData.Cost);
            CardCost totalCost = baseCost + cardModel.FeeChange;
            BattleAnimSystem.Instance.QueueResourceBarValChange(battleModel.PlayerModel.Resources + totalCost, 0.2f);
            // 返还费用给玩家
            battleModel.PlayerModel.SetResourcesWithoutNotify(battleModel.PlayerModel.Resources + totalCost);
            Debug.Log($"炼制卡牌事件：玩家返还费用 => {totalCost.Fire}/{totalCost.Water}/{totalCost.Earth}/{totalCost.Air}");
            if (cardData.EffectValue != null && cardData.EffectValue.Length >= 3)
            {
                var enemy = battleModel.EnemyModel;
                //设置血量
                var deltaHealth = cardData.EffectValue[0] + cardModel.AdditionEffectVal1;
                BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetHealthText().transform.position, deltaHealth);
                BattleAnimSystem.Instance.QueuePlayEnemyHealthChangeAnim(enemy.Health, enemy.Health + deltaHealth);
                enemy.SetHealthWithoutNotify(enemy.Health + deltaHealth);
                
                //设置攻击
                var deltaAttack = cardData.EffectValue[1] + cardModel.AdditionEffectVal2;
                var newAttack = enemy.Attack + deltaAttack;
                //敌方有HighReactivity时攻击不受影响
                if (battleModel.EnemyModel.CheckBuffByID(BuffIds.HighReactivity) == true)
                {
                    BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetAttackText().transform.position, 0);
                }
                else
                {
                    BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetAttackText().transform.position, deltaAttack);
                    if (newAttack <= 0)
                    {
                        BattleAnimSystem.Instance.QueueEnemyAttackChangeAnim(enemy.Attack, 1);
                        enemy.SetAttackWithoutNotify(1);
                    }
                    else
                    {
                        BattleAnimSystem.Instance.QueueEnemyAttackChangeAnim(enemy.Attack, enemy.Attack + deltaAttack);
                        enemy.SetAttackWithoutNotify(enemy.Attack + deltaAttack);
                    }
                }


                //设置奖励数
                var deltaReward = cardData.EffectValue[2] + cardModel.AdditionEffectVal3;
                BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetRewardText().transform.position, deltaReward);
                BattleAnimSystem.Instance.QueueEnemyRewardChangeAnim(enemy.Reward, enemy.Reward + deltaReward);
                enemy.SetRewardWithoutNotify(enemy.Reward + deltaReward);
                Debug.Log($"炼制卡牌事件：敌人属性增加 => 耐性+{cardData.EffectValue[0]}, 烈性+{cardData.EffectValue[1]}, 活性+{cardData.EffectValue[2]}");
            }

            // 先标记删除，再处理卡牌效果
            MarkCardForRemoval(handCardIndex);
            
            //清除乘风
            battleModel.PlayerModel.Tailwind = 0;
            BattleAnimSystem.Instance.PlayGaugesValueChangeAnim(0,0.1f);
            
            // 结算卡牌效果(场景=Sacrifice)
            ProcessCardEffects(battleModel, cardModel, cardData, CardUseContext.Sacrifice);
            BattleAnimSystem.Instance.QueueSacrificeCardFromHandAnim(handCardIndex);
            BattleAnimSystem.Instance.QueueSetCardVisibility(handCardIndex, false);
        }

        public static List<int> GetPendingRemovalIndices()
        {
            return _pendingRemovalIndices;
        }

        public static void DiscardCardFromHand(int handCardIndex)
        {
            var battleModel = DataModel<BattleModel>.Instance;
            if (handCardIndex < 0 || handCardIndex >= battleModel.HandCards.Count)
            {
                Debug.Log("DiscardCard：无效索引");
                return;
            }
            var cardModel = battleModel.HandCards[handCardIndex];
            var cardData = cardModel.Id.Data();

            Debug.Log($"丢弃手牌：索引 {handCardIndex}");
            MarkCardForRemoval(handCardIndex);

            // 结算卡牌效果(场景=Discard)
            ProcessCardEffects(battleModel, cardModel, cardData, CardUseContext.Discard);
            BattleAnimSystem.Instance.QueueSetCardVisibility(handCardIndex, false);
        }

        public static void DiscardAllHandCards()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            for (int i = battleModel.HandCards.Count - 1; i >= 0; i--)
            {
                MarkCardForRemoval(i);
            }
        }

        public static void EnemyAttack()
        {
            var enemyModel = DataModel<BattleModel>.Instance.EnemyModel;

            //眩晕时不可攻击
            if (enemyModel.CheckBuffByID(BuffIds.Stun))
                return;

            var playerModel = DataModel<BattleModel>.Instance.PlayerModel;
            var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
            BattleAnimSystem.Instance.QueueEnemyAttackPlayerAnim();
            //BattleAnimSystem.Instance.QueueDamagePopup(playerView.GetHealthText().transform.position, -enemyModel.Attack);
            //BattleAnimSystem.Instance.QueuePlayHealthChangeAnim(playerModel.Health, playerModel.Health - enemyModel.Attack);
            //playerModel.SetHealthWithoutNotify(playerModel.Health - enemyModel.Attack);

            EffectLogic.ApplyPlayerDamage(playerModel, enemyModel.Attack, true);
        }

        /// <summary>
        /// 执行所有 Buff 效果
        /// </summary>
        public static void ApplyBuffs()
        {
            var battleModel = DataModel<BattleModel>.Instance;

            // 对玩家的 Buff 进行结算
            BuffLogic.ProcessTurnBuffsForPlayer(battleModel.PlayerModel);
            // 对敌人 Buff 进行结算
            BuffLogic.ProcessTurnBuffsForEnemy(battleModel.EnemyModel);
        }

        /// <summary>
        /// 回合内结算效果
        /// </summary>
        public static void TurnSettlement()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            var playerModel = battleModel.PlayerModel;

            Debug.Log($"回合内结算效果：集中条件 => {playerModel.IsFocusCondition}, 集中参数 => {playerModel.FocusArgs}");
            // 处理集中条件
            if (playerModel.IsFocusCondition)
            {
                playerModel.IsFocusCondition = false;
                for(int i = 0; i < playerModel.FocusArgs; i++)
                {
                    DrawCardToHand();
                }
            }
            
        }

        /// <summary>
        /// 清理所有层数小于等于0的 Buff
        /// </summary>
        public static void RemoveUnavailableBuffs()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            BuffLogic.CleanUpBuffs(battleModel.PlayerModel.Buffs);
            BuffLogic.CleanUpBuffs(battleModel.EnemyModel.enemyBuffs);
        }

        /// <summary>
        /// 遍历卡牌配置的EffectIds和EffectValue，并调用EffectLogic.ProcessEffect进行结算。
        /// </summary>
        private static void ProcessCardEffects(BattleModel model, CardModel cardModel, CardTable.Entry cardData, CardUseContext usageContext)
        {
            var baseEffects = cardData.Effects;
            var baseValues = cardData.EffectValue;
            if (baseEffects == null) return;

            // 预处理：收集所有效果的条件检查结果
            List<bool> effectConditions = new List<bool>();
            for (int i = 0; i < baseEffects.Length; i++)
            {
                var effectId = baseEffects[i];
                if (effectId == EffectIds.Unknown)
                {
                    effectConditions.Add(false);
                    continue;
                }

                var effectData = effectId.Data();
                if (effectData == null)
                {
                    Debug.LogWarning($"EffectId {effectId} 的数据未找到，跳过处理。");
                    effectConditions.Add(false);
                    continue;
                }

                // 获取条件类型
                CardCondition condition = effectData.Condition;
                // 预处理阶段检查条件
                bool conditionMet = EffectLogic.CheckEffectCondition(model, cardModel, condition, usageContext, i);
                effectConditions.Add(conditionMet);
            }

            //使用预处理的条件结果处理每个效果
            for (int i = 0; i < baseEffects.Length; i++)
            {
                var effectId = baseEffects[i];
                if (effectId == EffectIds.Unknown)
                    continue;

                var effectData = effectId.Data();
                if (effectData == null)
                {
                    Debug.LogWarning($"EffectId {effectId} 的数据未找到，跳过处理。");
                    continue;
                }

                int baseArg = 0;
                if (baseValues != null && i < baseValues.Length)
                    baseArg = baseValues[i];

                // 加上卡牌模型中额外的 effectValue
                int additionArg = 0;
                switch (i)
                {
                    case 0: additionArg = cardModel.AdditionEffectVal1; break;
                    case 1: additionArg = cardModel.AdditionEffectVal2; break;
                    case 2: additionArg = cardModel.AdditionEffectVal3; break;
                }
                int finalArg = baseArg + additionArg;


                bool conditionMet = effectConditions[i];
                // 调用 EffectLogic 处理效果，传入 effectIndex = i
                EffectLogic.ProcessEffect(model, cardModel, effectId, finalArg, i, usageContext, conditionMet);
            }
            
        }

        public static CardModel[] GetRandomNewCard(int num, bool isRareOnly = false, bool includeShop = false)
        {
            var cardTable = DataSystem.Instance.Load<CardTable>();
            var playerPool = DataModel<BattleModel>.Instance.PlayerModel.CharacterId.Data().CharElemental;

            if (cardTable == null || cardTable.Length == 0)
            {
                Debug.LogError("CardTable is null or empty.");
                return new CardModel[0];
            }

            List<CardTable.Entry> validEntries = new List<CardTable.Entry>();
            for (int i = 0; i < cardTable.Length; i++)
            {
                var entry = (CardTable.Entry)cardTable.Get(i);
                if (IsCardInPlayerPool(entry, playerPool))
                    validEntries.Add(entry);
            }

            // 是否添加商店牌
            if (!includeShop)
                validEntries = validEntries.Where(e => e.Rarity != CardRarity.Shop).ToList();

            // 过滤稀有
            if (isRareOnly)
            {
                validEntries = validEntries.Where(e => e.Rarity == CardRarity.Rare).ToList();
                if (validEntries.Count == 0) return Array.Empty<CardModel>();
            }

            if (validEntries.Count == 0)
            {
                Debug.LogWarning("No card entry found that is in the player's pool.");
                return new CardModel[0];
            }

            // 计算权重逻辑保持不变
            int rCount = validEntries.Count(e => e.Rarity == CardRarity.Rare);
            int nCount = validEntries.Count - rCount;

            List<float> weights = new List<float>();
            foreach (var entry in validEntries)
            {
                if (rCount > 0 && nCount > 0)
                {
                    weights.Add(entry.Rarity == CardRarity.Rare ? 0.03f / rCount : 0.97f / nCount);
                }
                else
                {
                    weights.Add(1.0f / validEntries.Count);
                }
            }

            float totalWeight = weights.Sum();

            List<CardModel> cardModels = new List<CardModel>();
            for (int i = 0; i < num; i++)
            {
                float random = UnityEngine.Random.Range(0, totalWeight);
                float current = 0;
                int selectedIndex = 0;
                for (int j = 0; j < validEntries.Count; j++)
                {
                    current += weights[j];
                    if (current >= random)
                    {
                        selectedIndex = j;
                        break;
                    }
                }

                var selectedEntry = validEntries[selectedIndex];
                var card = new CardModel();
                card.Id = Enum.TryParse(selectedEntry.Id, out CardIds result) ? result : default(CardIds);
                cardModels.Add(card);

                // 更新总权重（因为条目被移除）
                totalWeight -= weights[selectedIndex];
                validEntries.RemoveAt(selectedIndex);
                weights.RemoveAt(selectedIndex);
            }

            return cardModels.ToArray();
        }

        /// <summary>
        /// 检查一个卡牌表项是否在玩家的卡池中
        /// </summary>
        private static bool IsCardInPlayerPool(CardTable.Entry entry, bool[] playerPool)
        {
            if (entry == null || playerPool == null)
                return false;

            bool hasAtLeastOnePool = false;

            for (int i = 0; i < entry.Pool.Length; i++)
            {
                if (entry.Pool[i])
                {
                    hasAtLeastOnePool = true; // 标记卡牌有至少一个元素池
                    if (!playerPool[i])
                        return false; // 玩家未开放该元素池
                }
            }

            // 卡牌的 Pool 全为 false，视为无效
            return hasAtLeastOnePool;
        }

        public static void RecycleCard()
        {
            var battleModel = DataModel<BattleModel>.Instance;
            if (!CanDrawCard()) return;
            // 如果剩余牌堆为空，则将弃牌堆洗牌回剩余牌堆
            if (battleModel.DiscardCards.Count == 0)
            {
                Debug.Log("弃牌堆为空，无法回收。");
                return;
            }
            Debug.Log("回收事件：从弃牌堆回收一张卡牌");
            Shuffle(battleModel.DiscardCards);
            // 从 DiscardCards 中取出第一张
            var recycleCard = battleModel.DiscardCards[0];
            battleModel.DiscardCards.RemoveAt(0);
            battleModel.HandCards.Add(recycleCard);
            // 添加动画队列，参数为新加入的卡牌索引
            BattleAnimSystem.Instance.QueueDrawCardToHandAnim(battleModel.HandCards.Count - 1);
        }

    }
}
