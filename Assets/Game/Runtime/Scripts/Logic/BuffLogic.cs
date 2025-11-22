using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using WanFramework.UI.DataComponent;
using Game.BattleAnim;
using Game.UI.Common.Components;
using Game.UI.InGame.Components;
using WanFramework.Data;
using System.Linq;
using Game.UI.InGame;
using WanFramework.UI;
using System.Collections;

namespace Game.Logic
{
    /// <summary>
    /// Buff 逻辑管理，用于添加、叠加、递减、驱散以及每回合结算 Buff 效果。
    /// 分开处理玩家和敌人的 Buff 结算逻辑。
    /// </summary>
    public static class BuffLogic
    {

        private struct DecreaseRequest
        {
            public DataModelBase TargetModel;
            public DataCollection<BuffModel> Collection;
            public BuffIds BuffId;
            public int Amount;
        }


        //延迟减少记录器
        private static List<DecreaseRequest> pendingDecreases = new();

        #region 基础方法

        /// <summary>
        /// 在 buffCollection 中添加或叠加一个 Buff。
        /// </summary>
        public static void ApplyBuff(DataModelBase targetModel, BuffIds buffId, int stackIncrease = 1)
        {
            DataCollection<BuffModel> buffCollection = null;
            if (targetModel is PlayerModel player)
                buffCollection = player.Buffs;
            else if (targetModel is EnemyModel enemy)
                buffCollection = enemy.enemyBuffs;

            var buffData = buffId.Data();

            bool found = false;
            for (int i = 0; i < buffCollection.Count; i++)
            {
                if (buffCollection[i].Id == buffId)
                {
                    var buff = buffCollection[i];
                    var stack = Math.Min(buff.Stack + stackIncrease, buffData.StackMaximum);
                    
                    found = true;


                    if (targetModel is PlayerModel)
                    {
                        BattleAnimSystem.Instance.QueueBuffStackPopup(i, stackIncrease);
                        BattleAnimSystem.Instance.QueueBuffStackAddAnim(i);
                        BattleAnimSystem.Instance.QueueBuffStackNumChange(i, buff.Stack, stack);
                    }
                    else if (targetModel is EnemyModel)
                    {
                        BattleAnimSystem.Instance.QueueEnemyBuffStackPopup(i, stackIncrease);
                        BattleAnimSystem.Instance.QueueEnemyBuffStackAddAnim(i);
                        BattleAnimSystem.Instance.QueueEnemyBuffStackNumChange(i, buff.Stack, stack);
                    }

                    buffCollection[i].SetStackWithoutNotify(stack);
                    
                    Debug.Log($"Buff {buffId} 叠加至 {buff.Stack}");
                    break;
                }
            }
            if (!found)
            {
                var stack = Math.Min(stackIncrease, buffData.StackMaximum);

                var newBuff = new BuffModel
                {
                    Id = buffId,
                    Stack = 0
                };

                buffCollection.Add(newBuff);

                if (targetModel is PlayerModel)
                {
                    BattleAnimSystem.Instance.QueueBuffShowAnim(buffCollection.Count - 1);
                    BattleAnimSystem.Instance.QueueBuffStackPopup(buffCollection.Count - 1, stackIncrease);
                    BattleAnimSystem.Instance.QueueBuffStackAddAnim(buffCollection.Count - 1);
                    BattleAnimSystem.Instance.QueueBuffStackNumChange(buffCollection.Count - 1, 0, stack);
                }
                else if (targetModel is EnemyModel)
                {
                    BattleAnimSystem.Instance.QueueEnemyBuffShowAnim(buffCollection.Count - 1);
                    BattleAnimSystem.Instance.QueueEnemyBuffStackPopup(buffCollection.Count - 1, stackIncrease);
                    BattleAnimSystem.Instance.QueueEnemyBuffStackAddAnim(buffCollection.Count - 1);
                    BattleAnimSystem.Instance.QueueEnemyBuffStackNumChange(buffCollection.Count - 1, 0, stack);
                }

                //之后再设置数值
                buffCollection[buffCollection.Count - 1].SetStackWithoutNotify(stack);

                Debug.Log($"Buff {buffId} 添加，初始层数 {newBuff.Stack}");
            }
        }

        /// <summary>
        /// 递减指定 Buff 的层数（延迟执行）
        /// </summary>
        public static void DecreaseBuff(DataModelBase targetModel, DataCollection<BuffModel> buffCollection, BuffIds buffId, int decrease = 1)
        {
            // 检查是否为Boss Buff（不可递减）
            var buffData = buffId.Data();
            if (buffData.BuffDecreaseAbility == false) 
                return;

            // 查找当前实际层数
            var currentBuff = buffCollection.FirstOrDefault(b => b.Id == buffId);
            if (currentBuff == null || currentBuff.Stack <= 0)
            {
                Debug.Log($"[BUFF] {buffId} 当前层数不足，跳过递减");
                return;
            }

            // 修正实际递减量
            var actualDecrease = Mathf.Min(decrease, currentBuff.Stack);
            pendingDecreases.Add(new DecreaseRequest
            {
                TargetModel = targetModel,
                Collection = buffCollection,
                BuffId = buffId,
                Amount = actualDecrease // 使用修正后的值
            });
        }

        public static void ProcessDecreaseBuff(DataCollection<BuffModel> buffCollection)
        {
            //处理该集合的所有延迟减少
            foreach (var req in pendingDecreases.Where(r => r.Collection == buffCollection).ToList())
            {
                for (int i = 0; i < req.Collection.Count; i++)
                {
                    var buff = req.Collection[i];
                    if (buff.Id == req.BuffId)
                    {
                        var oldStack = buff.Stack;
                        var newStack = Mathf.Max(oldStack - req.Amount, 0);
                        req.Collection[i].SetStackWithoutNotify(newStack);
                        
                        break;
                    }
                }
                pendingDecreases.Remove(req); // 处理完移除
            }
        }

        /// <summary>
        /// 清理集合中所有层数小于等于0的 Buff。
        /// </summary>
        public static void CleanUpBuffs(DataCollection<BuffModel> buffCollection)
        {
            ProcessDecreaseBuff(buffCollection);

            for (int i = buffCollection.Count - 1; i >= 0; i--)
            {
                //如果Buff 允许驱散，则从集合中移除该 Buff
                if (buffCollection[i].Stack <= 0 && buffCollection[i].Id.Data().BuffDispelAbility == true)
                {
                    buffCollection.RemoveAt(i);
                }
            }
        }

        #endregion

        #region 玩家 Buff 结算

        /// <summary>
        /// 定义玩家 Buff 效果的委托。
        /// 参数：PlayerModel 对象、当前 Buff 信息
        /// </summary>
        private delegate void PlayerBuffEffect(PlayerModel player, BuffModel buff, int buffIndex);

        /// <summary>
        /// 玩家 Buff 处理字典
        /// </summary>
        private static readonly Dictionary<BuffIds, PlayerBuffEffect> playerBuffEffects = new Dictionary<BuffIds, PlayerBuffEffect>()
        {
            {
                BuffIds.Burning,
                (player, buff, buffIndex) =>
                {
                    int damage = buff.Stack;
                    //var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
                    //BattleAnimSystem.Instance.QueueDamagePopup(playerView.GetHealthText().transform.position, - damage);
                    BattleAnimSystem.Instance.QueueBuffEffect(buffIndex);
                    //BattleAnimSystem.Instance.QueuePlayHealthChangeAnim(player.Health, player.Health - damage);
                    //player.SetHealthWithoutNotify(player.Health - damage);
                    EffectLogic.ApplyPlayerDamage(player, damage);
                    Debug.Log($"[BUFF] 燃烧：玩家受到 {damage} 点伤害。");
                }
            },
            {
                BuffIds.Quickness,
                (player, buff, buffIndex) =>
                {
                    BattleLogic.DrawCardToHand();
                    Debug.Log("[BUFF] 迅捷：玩家抽 1 张牌。");
                }
            },
            {
                BuffIds.Stun,
                (player, buff, buffIndex) =>
                {
                    //EffectLogic.DiscardCards(1);
                    //Debug.Log("[BUFF] 晕眩：随机丢弃玩家一张手牌。");
                }
            },
            {
                BuffIds.Calm,
                (player, buff, buffIndex) =>
                {
                    Debug.Log("[BUFF] 冷静：使用不稳定的卡牌能够稳定");
                }
            },
            {
                BuffIds.Sturdy,
                (player, buff, buffIndex) =>{ }
            },
            {
                BuffIds.Reflect,
                (player, buff, buffIndex) =>{ }
            }
        };

        /// <summary>
        /// 对玩家所有 Buff 进行每回合结算处理：调用对应的处理委托，
        /// 调用对应的处理委托后，直接播放对应 BuffView 的 BuffStackChangeAnim 动画，
        /// 然后统一递减该 Buff 的层数 1。
        /// </summary>
        public static void ProcessTurnBuffsForPlayer(PlayerModel player)
        {
            var buffs = player.Buffs;
            // 遍历时 buffViews 子视图顺序与 buffs 顺序一致
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                BuffModel buff = buffs[i];
                if (playerBuffEffects.TryGetValue(buff.Id, out PlayerBuffEffect effect))
                {
                    effect(player, buff, i);
                }
                else
                {
                    if (buff.Id.Data().BuffDecreaseAbility == false)
                        return;

                    Debug.LogWarning($"[BUFF] 玩家未处理的 Buff: {buff.Id}");
                }
                // 仅当层数 > 0 时播放动画
                if (buff.Stack > 0)
                {
                    BattleAnimSystem.Instance.QueueBuffStackPopup(i, -1);
                    BattleAnimSystem.Instance.QueueBuffStackMinusAnim(i);
                    BattleAnimSystem.Instance.QueueBuffStackNumChange(i, buff.Stack, buff.Stack - 1);

                    // 真正延迟减一
                    DecreaseBuff(player, buffs, buff.Id, 1);
                }
            }

            ProcessDecreaseBuff(player.Buffs);
        }



        #endregion

        #region 敌人 Buff 结算

        /// <summary>
        /// 定义敌人 Buff 效果的委托。
        /// </summary>
        private delegate void EnemyBuffEffect(EnemyModel enemy, BuffModel buff, int buffIndex);

        /// <summary>
        /// 敌人 Buff 处理字典
        /// </summary>
        private static readonly Dictionary<BuffIds, EnemyBuffEffect> enemyBuffEffects = new Dictionary<BuffIds, EnemyBuffEffect>()
        {
            {
                BuffIds.Burning,
                (enemy, buff, buffIndex) =>
                {
                    int damage = buff.Stack;
                    //var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
                    //BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetHealthText().transform.position, - damage);
                    BattleAnimSystem.Instance.QueueEnemyBuffEffect(buffIndex);
                    //敌人Buff受伤动画
                    BattleAnimSystem.Instance.QueueEnemyTakeBuffDamageAnim();
                    //BattleAnimSystem.Instance.QueuePlayEnemyHealthChangeAnim(enemy.Health, enemy.Health - damage);
                    //enemy.SetHealthWithoutNotify(enemy.Health - damage);
                    EffectLogic.ApplyEnemyDamage(enemy, damage);

                    Debug.Log($"[BUFF] 燃烧（敌人）：敌人受到 {damage} 点伤害。");
                }
            },
            {
                BuffIds.Stun,
                (enemy, buff, buffIndex) =>
                {
                    Debug.Log("[BUFF] 晕眩（敌人）");
                }
            },
            {
                BuffIds.HighReactivity,
                (enemy, buff, buffIndex) =>
                {
                    //攻击为Buff层数
                    enemy.Attack = buff.Stack;
                }
            },
            {
                BuffIds.Sturdy,
                (enemy, buff, buffIndex) =>{ }
            },
            {
                BuffIds.Reflect,
                (enemy, buff, buffIndex) =>{ }
            }
        };

        /// <summary>
        /// 对敌人所有 Buff 进行每回合结算处理：
        /// 对每个 Buff 调用对应的处理委托后，播放对应 BuffView 的 EnemyBuffStackChangeAnim 动画，
        /// 最后统一递减该 Buff 的层数 1。
        /// </summary>
        /// <param name="enemy">敌人模型</param>
        /// <param name="buffViews">敌人 Buff 的集合视图</param>
        public static void ProcessTurnBuffsForEnemy(EnemyModel enemy)
        {
            var buffs = enemy.enemyBuffs;
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                BuffModel buff = buffs[i];
                var buffData = buff.Id.Data();

                // 执行 Buff 效果
                if (enemyBuffEffects.TryGetValue(buff.Id, out EnemyBuffEffect effect))
                {
                    effect(enemy, buff, i);
                }

                // 检查是否可递减
                if (buffData.BuffDecreaseAbility == false)
                {
                    Debug.Log($"[BUFF] {buff.Id} 不可递减，跳过动画和层数变化");
                    continue; // 跳过当前 Buff 的后续处理
                }

                // 仅当层数 > 0 时播放动画
                if (buff.Stack > 0)
                {
                    BattleAnimSystem.Instance.QueueEnemyBuffStackPopup(i, -1);
                    BattleAnimSystem.Instance.QueueEnemyBuffStackMinusAnim(i);
                    BattleAnimSystem.Instance.QueueEnemyBuffStackNumChange(i, buff.Stack, buff.Stack - 1);

                    DecreaseBuff(enemy, buffs, buff.Id, 1);
                }
            }

            ProcessDecreaseBuff(enemy.enemyBuffs);
        }



        #endregion

        #region BossBuff逻辑结算

        /// <summary>
        /// 定义Boss Buff 效果的委托。
        /// </summary>
        private delegate void BossBuffEffect(BattleModel battleModel, BuffModel buff);

        /// <summary>
        /// Boss Buff 处理字典
        /// </summary>
        private static readonly Dictionary<BuffIds, BossBuffEffect> bossBuffEffects = new Dictionary<BuffIds, BossBuffEffect>()
        {
            {
                BuffIds.Rusted,
                (battleModel, buff) =>
                {
                    //修改伤害为Buff层数
                    int damage = buff.Stack;
                    var player = battleModel.PlayerModel;
                    var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
                    BattleAnimSystem.Instance.QueueDamagePopup(playerView.GetHealthText().transform.position, - damage);
                    BattleAnimSystem.Instance.QueuePlayHealthChangeAnim(player.Health, player.Health - damage);
                    player.SetHealthWithoutNotify(player.Health - damage);
                    Debug.Log($"[BOSS BUFF] Rusted 触发");
                }
            },
        };

        /// <summary>
        /// 玩家使用卡牌时触发的Boss Buff效果
        /// </summary>
        public static void TriggerBossBuffOnCardUse(BattleModel battleModel)
        {
            var enemyBuffs = battleModel.EnemyModel.enemyBuffs;

            foreach (var buff in enemyBuffs)
            {
                if (bossBuffEffects.TryGetValue(buff.Id, out var effect))
                {
                    effect(battleModel, buff);
                    // 不进行层数递减
                    Debug.Log($"[BOSS BUFF] {buff.Id} 触发效果");
                }
            }
        }

        #endregion
        
    }
}