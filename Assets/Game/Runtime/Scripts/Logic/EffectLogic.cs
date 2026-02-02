using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Game.BattleAnim;
using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.InGame;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WanFramework.UI;
using WanFramework.UI.DataComponent;
using static Game.Logic.BattleLogic;

namespace Game.Logic
{
    public static class EffectLogic
    {
        private delegate void EffectHandler(BattleModel model, CardModel cardModel, EffectIds effectId, int effectArg, int effectIndex, CardUseContext usageContext);

        private static Dictionary<EffectIds, EffectHandler> _effectHandlers = new()
        {
            {
                EffectIds.Unknown,
                (m, cm, id, arg, index, usageContext) =>
                {

                }
            },
            {
                EffectIds.Damage,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyEnemyDamage(m.EnemyModel, arg, true);
                }
            },
            {
                EffectIds.Damage_Self,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyPlayerDamage(m.PlayerModel, arg);
                }
            },
            {
                EffectIds.Heal,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyHeal(m, arg);
                }
            },
            {
                EffectIds.Draw,
                (m, cm, id, arg, index, usageContext) =>
                {
                    DrawCards(arg);
                }
            },
            {
                EffectIds.Recycle,
                (m, cm, id, arg, index, usageContext) =>
                {
                    for(int i = 0; i < arg; i++)
                    {
                        BattleLogic.RecycleCard();
                    }
                    Debug.Log($"[Effect] Draw: 玩家回收了 {arg} 张牌");
                }
            },
            {
                EffectIds.Discard,
                (m, cm, id, arg, index, usageContext) =>
                {
                    DiscardCards(arg);
                }
            },
            {
                // 调和
                EffectIds.Neutralize,
                (m, cm, id, arg, index, usageContext) =>
                {
                    var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");

                    var enemy = m.EnemyModel;
                    int newHP = enemy.Health;
                    int newATK = enemy.Attack;
                    int newREW = enemy.Reward;

                    switch(index)
                    {
                        case 0: // HP
                            newHP -= arg;
                            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetHealthText().transform.position, - arg);
                            //敌人受伤动画
                            BattleAnimSystem.Instance.QueueEnemyTakeDamageAnim();
                            BattleAnimSystem.Instance.QueuePlayEnemyHealthChangeAnim(enemy.Health, newHP);
                            enemy.SetHealthWithoutNotify(newHP);
                            Debug.Log($"[Effect] Neutralize: 敌人HP -{arg} => HP {newHP}");
                            break;
                        case 1: // Attack
                            newATK -= arg;
                            
                            //敌方有HighReactivity时攻击不受影响
                            if (m.EnemyModel.CheckBuffByID(BuffIds.HighReactivity) == true)
                            {
                                BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetAttackText().transform.position, 0);
                                break;
                            }
                            
                            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetAttackText().transform.position, - arg);
                            //攻击力不能低于0
                            if (newATK <= 0)
                            {
                                enemy.SetAttackWithoutNotify(1);
                                BattleAnimSystem.Instance.QueueEnemyAttackChangeAnim(enemy.Attack, 1);
                            }
                            else
                            {
                                BattleAnimSystem.Instance.QueueEnemyAttackChangeAnim(enemy.Attack, newATK);
                                enemy.SetAttackWithoutNotify(newATK);
                            }
                            Debug.Log($"[Effect] Neutralize: 敌人Attack -{arg} => ATK {newATK}");
                            break;
                        case 2: // Reward
                            newREW -= arg;
                            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetRewardText().transform.position, - arg);
                            BattleAnimSystem.Instance.QueueEnemyRewardChangeAnim(enemy.Reward, newREW);
                            enemy.SetRewardWithoutNotify(newREW);
                            Debug.Log($"[Effect] Neutralize: 敌人Reward -{arg} => REW {newREW}");
                            break;
                        default:
                            Debug.LogWarning($"[Effect] Neutralize: 无效的 effectIndex {index}");
                            break;
                    }
                }
            },
            {
                // 催化
                EffectIds.Catalyze,
                (m, cm, id, arg, index, usageContext) =>
                {
                    var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
                    var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");

                    var enemy = m.EnemyModel;
                    int newHP = enemy.Health;
                    int newATK = enemy.Attack;
                    int newREW = enemy.Reward;

                    switch(index)
                    {
                        case 0: // HP
                            newHP += arg;
                            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetHealthText().transform.position, arg);
                            BattleAnimSystem.Instance.QueuePlayEnemyHealthChangeAnim(enemy.Health, newHP);
                            enemy.SetHealthWithoutNotify(newHP);
                            Debug.Log($"[Effect] Catalyze: 敌人HP +{arg} => HP {newHP}");
                            break;
                        case 1: // Attack
                            newATK += arg;
                            
                            //敌方有HighReactivity时攻击不受影响
                            if (m.EnemyModel.CheckBuffByID(BuffIds.HighReactivity) == true)
                            {
                                BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetAttackText().transform.position, 0);
                                break;
                            }
                            
                            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetAttackText().transform.position, arg);
                            //攻击力不能低于0
                            if (newATK <= 0)
                            {
                                enemy.SetAttackWithoutNotify(0);
                                BattleAnimSystem.Instance.QueueEnemyAttackChangeAnim(enemy.Attack, 0);
                            }
                            else
                            {
                                BattleAnimSystem.Instance.QueueEnemyAttackChangeAnim(enemy.Attack, newATK);
                                enemy.SetAttackWithoutNotify(newATK);
                            }
                            Debug.Log($"[Effect] Catalyze: 敌人Attack +{arg} => ATK {newATK}");
                            break;
                        case 2: // Reward
                            newREW += arg;
                            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetRewardText().transform.position, arg);
                            BattleAnimSystem.Instance.QueueEnemyRewardChangeAnim(enemy.Reward, newREW);
                            enemy.SetRewardWithoutNotify(newREW);
                            Debug.Log($"[Effect] Catalyze: 敌人Reward +{arg} => REW {newREW}");
                            break;
                        default:
                            Debug.LogWarning($"[Effect] Catalyze: 无效的 effectIndex {index}");
                            break;
                    }
                }
            },
            {
                EffectIds.Elemental_Fire,
                (m, cm, id, arg, index, usageContext) =>
                {
                    AddResource(m, CardBaseType.Fire, arg);
                    Debug.Log($"[Effect] Elemental_Fire: 增加 {arg} 个火元素");
                }
            },
            {
                EffectIds.Elemental_Water,
                (m, cm, id, arg, index, usageContext) =>
                {
                    AddResource(m, CardBaseType.Water, arg);
                    Debug.Log($"[Effect] Elemental_Water: 增加 {arg} 个水元素");
                }
            },
            {
                EffectIds.Elemental_Earth,
                (m, cm, id, arg, index, usageContext) =>
                {
                    AddResource(m, CardBaseType.Earth, arg);
                    Debug.Log($"[Effect] Elemental_Earth: 增加 {arg} 个土元素");
                }
            },
            {
                EffectIds.Elemental_Air,
                (m, cm, id, arg, index, usageContext) =>
                {
                    AddResource(m, CardBaseType.Air, arg);
                    Debug.Log($"[Effect] Elemental_Air: 增加 {arg} 个气元素");
                }
            },
            {
                EffectIds.Idle,
                (m, cm, id, arg, index, usageContext) =>
                {
                    Debug.Log($"[Effect] Idle: 啥都不干, 但数值是 {arg}");
                }
            },
            {
                EffectIds.Scaling,
                (m, cm, id, arg, index, usageContext) =>
                {
                    m.AdditionalMaxHandCount += arg;
                }
            },
            //Surge
            {
                EffectIds.Damage_Surge,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            {
                EffectIds.Damage_Self_Surge,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyPlayerDamage(m.PlayerModel, arg);
                }
            },
            {
                EffectIds.Heal_Surge,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyHeal(m, arg);
                }
            },
            {
                EffectIds.Draw_Surge,
                (m, cm, id, arg, index, usageContext) =>
                {
                    DrawCards(arg);
                }
            },
            //Madness
            {
                EffectIds.Damage_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            {
                EffectIds.Damage_Self_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyPlayerDamage(m.PlayerModel, arg);
                }
            },
            {
                EffectIds.Heal_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyHeal(m, arg);
                }
            },
            {
                EffectIds.Draw_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    DrawCards(arg);
                }
            },
            {
                EffectIds.Discard_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    DiscardCards(arg);
                }
            },
            {
                EffectIds.Elemental_Water_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    AddResource(m, CardBaseType.Water, arg);
                    Debug.Log($"[Effect] Elemental_Water: 增加 {arg} 个水元素");
                }
            },
            //Unstable
            {
                EffectIds.Damage_Unstable,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            {
                EffectIds.Damage_Self_Unstable,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyPlayerDamage(m.PlayerModel, arg);
                }
            },
            {
                EffectIds.Heal_Unstable,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyHeal(m, arg);
                }
            },
            {
                EffectIds.Draw_Unstable,
                (m, cm, id, arg, index, usageContext) =>
                {
                    DrawCards(arg);
                }
            },
            //Cooldown
            {
                EffectIds.Damage_Cooldown,
                (m, cm, id, arg, index, usageContext) =>
                {
                    bool hadBurning = m.PlayerModel.CheckBuffByID(BuffIds.Burning);
                    if (hadBurning)
                    {
                        // 清除所有燃烧 Buff
                        var buffs = m.PlayerModel.Buffs;
                        for (int i = buffs.Count - 1; i >= 0; i--)
                        {
                            if (buffs[i].Id == BuffIds.Burning)
                                buffs.RemoveAt(i);
                        }
                    }

                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            {
                EffectIds.Draw_Cooldown,
                (m, cm, id, arg, index, usageContext) =>
                {
                    bool hadBurning = m.PlayerModel.CheckBuffByID(BuffIds.Burning);
                    if (hadBurning)
                    {
                        // 清除所有燃烧 Buff
                        var buffs = m.PlayerModel.Buffs;
                        for (int i = buffs.Count - 1; i >= 0; i--)
                        {
                            if (buffs[i].Id == BuffIds.Burning)
                                buffs.RemoveAt(i);
                        }
                    }

                    DrawCards(arg);
                }
            },
            {
                EffectIds.Ignite_Cooldown,
                (m, cm, id, arg, index, usageContext) =>
                {
                    if (m.EnemyModel.CheckBuffByID(BuffIds.Burning))
                        arg *= 2;

                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            {
                EffectIds.Calm_Cooldown,
                (m, cm, id, arg, index, usageContext) =>
                {
                    bool hadBurning = m.PlayerModel.CheckBuffByID(BuffIds.Burning);
                    if (hadBurning)
                    {
                        // 清除所有燃烧 Buff
                        var buffs = m.PlayerModel.Buffs;
                        for (int i = buffs.Count - 1; i >= 0; i--)
                        {
                            if (buffs[i].Id == BuffIds.Burning)
                                buffs.RemoveAt(i);
                        }
                    }

                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Calm, arg);
                    Debug.Log($"[Effect] Buff_Calm: 玩家增加 {arg} 层冷静");
                }
            },
            //Peace
            {
                EffectIds.Damage_Peace,
                (m, cm, id, arg, index, usageContext) =>
                {
                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            {
                EffectIds.Draw_Peace,
                (m, cm, id, arg, index, usageContext) =>
                {
                    for(int i = 0; i < arg; i++)
                    {
                        BattleLogic.DrawCardToHand();
                    }
                    Debug.Log($"[Effect] Draw: 玩家抓了 {arg} 张牌");
                }
            },
            {
                EffectIds.Quickness_Peace,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Quickness, arg);
                    Debug.Log($"[Effect] Buff_Quickness: 玩家增加 {arg} 层敏捷");
                }
            },
            {
                EffectIds.Calm_Peace,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Calm, arg);
                    Debug.Log($"[Effect] Buff_Calm: 玩家增加 {arg} 层冷静");
                }
            },
            //LED
            {
                EffectIds.LED_Water,
                (m, cm, id, arg, index, usageContext) =>
                {
                    //丢弃所有手牌
                    DiscardCards(m.HandCards.Count);

                    AddResource(m, CardBaseType.Water, arg);
                }
            },
            //Ignite
            {
                EffectIds.Ignite,
                (m, cm, id, arg, index, usageContext) =>
                {
                    if (m.EnemyModel.CheckBuffByID(BuffIds.Burning))
                        arg *= 2;

                    ApplyEnemyDamage(m.EnemyModel, arg);
                }
            },
            //Focus
            {
                EffectIds.Draw_Focus,
                (m, cm, id, arg, index, usageContext) =>
                {
                    m.PlayerModel.IsFocusCondition = true;
                    m.PlayerModel.FocusArgs = arg;
                    for(int i = 0; i < arg; i++)
                    {
                        BattleLogic.DrawCardToHand();
                    }
                    Debug.Log($"[Effect] Draw_Focus: 设置受伤标志位");
                }
            },
            {            
                EffectIds.Damage_Focus,
                (m, cm, id, arg, index, usageContext) =>
                {
                    m.PlayerModel.IsFocusCondition = true;
                    m.PlayerModel.FocusArgs = arg;
                    ApplyEnemyDamage(m.EnemyModel, arg);
                    Debug.Log($"[Effect] Draw_Focus: 设置受伤标志位");
                }
            },
            
            //Tailwind
            {
                EffectIds.Damage_Windborne,
                (m, cm, id, arg, index, usageContext) =>
                {
                    if (m.PlayerModel.Tailwind != 0)
                    {
                        ApplyEnemyDamage(m.EnemyModel, arg * m.PlayerModel.Tailwind);
                    }
                    Debug.Log($"乘风{m.PlayerModel.Tailwind}，总伤害为{arg * m.PlayerModel.Tailwind}");
                }
            },

            //Buff
            {
                EffectIds.Buff_Calm_Self,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Calm, arg);
                }
            },
            {
                EffectIds.Buff_Quickness_Self,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Quickness, arg);
                }
            },
            {
                EffectIds.Buff_Stun_Enemy,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.EnemyModel, BuffIds.Stun, arg);
                }
            },
            {
                EffectIds.Buff_Stun_Enemy_Madness,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.EnemyModel, BuffIds.Stun, arg);
                }
            },
            {
                EffectIds.Buff_Burn_Enemy,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.EnemyModel, BuffIds.Burning, arg);
                    Debug.Log($"[Effect] Buff_Burn_Enemy: 给敌人增加 {arg} 层燃烧");
                }
            },
            {
                EffectIds.Buff_Burn_Self,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Burning, arg);
                    Debug.Log($"[Effect] Buff_Burn_Self: 玩家增加 {arg} 层燃烧");
                }
            },
            {
                EffectIds.Buff_Sturdy_Self,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Sturdy, arg);
                    Debug.Log($"[Effect] Buff_Sturdy_Self: 玩家增加 {arg} 层坚固");
                }
            },
            {
                EffectIds.Buff_Reflect_Self,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.PlayerModel, BuffIds.Reflect, arg);
                    Debug.Log($"[Effect] Buff_Reflect_Self: 玩家增加 {arg} 层反射");
                }
            },
            {
                EffectIds.Buff_Reflect_Enemy,
                (m, cm, id, arg, index, usageContext) =>
                {
                    BuffLogic.ApplyBuff(m.EnemyModel, BuffIds.Reflect, arg);
                    Debug.Log($"[Effect] Buff_Reflect_Enemy: 敌人增加 {arg} 层反射");
                }
            }
        };

        #region 公共方法
        // 对敌人造成伤害
        public static void ApplyEnemyDamage(EnemyModel enemyModel, int damage, bool fromEntity = false)
        {
            // 1. 读取“坚固”层数
            int sturdyStack = 0, sturdyIdx = -1;
            for (int i = 0; i < enemyModel.enemyBuffs.Count; i++)
            {
                if (enemyModel.enemyBuffs[i].Id == BuffIds.Sturdy)
                {
                    sturdyStack = enemyModel.enemyBuffs[i].Stack;
                    sturdyIdx = i;
                    break;
                }
            }

            // 2. 计算实际伤害
            int actualDamage = Mathf.Max(damage - sturdyStack, 0);

            // 3. 播放伤害动画
            var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
            BattleAnimSystem.Instance.QueueDamagePopup(enemyView.GetHealthText().transform.position, -actualDamage);
            BattleAnimSystem.Instance.QueueShakeCameraAnim(0.5f);
            BattleAnimSystem.Instance.QueueEnemyTakeDamageAnim();
            BattleAnimSystem.Instance.QueuePlayEnemyHealthChangeAnim(
                enemyModel.Health, enemyModel.Health - actualDamage
            );
            enemyModel.SetHealthWithoutNotify(enemyModel.Health - actualDamage);

            Debug.Log($"[Effect] 敌人受到 {actualDamage} 点伤害 (原伤害{damage}，坚固抵消{sturdyStack})");

            // 4. 如果存在坚固，就层数减半
            if (sturdyIdx >= 0 && sturdyStack > 0)
            {
                int newStack = Mathf.CeilToInt(sturdyStack / 2f);
                enemyModel.enemyBuffs[sturdyIdx].SetStackWithoutNotify(newStack);

                BattleAnimSystem.Instance.QueueEnemyBuffStackPopup(sturdyIdx, newStack - sturdyStack);
                BattleAnimSystem.Instance.QueueEnemyBuffStackNumChange(
                    sturdyIdx, sturdyStack, newStack
                );

                Debug.Log($"[Buff Sturdy] 敌人坚固从 {sturdyStack} 减至 {newStack}");
            }

            if(fromEntity)
            {
                int reflectStack = 0, reflectIdx = -1;
                for (int i = 0; i < enemyModel.enemyBuffs.Count; i++)
                {
                    if (enemyModel.enemyBuffs[i].Id == BuffIds.Reflect)
                    {
                        reflectStack = enemyModel.enemyBuffs[i].Stack;
                        reflectIdx = i;
                        break;
                    }
                }

                int reflectDamage = Mathf.Max(reflectStack, 0);

                if(reflectIdx >= 0 && reflectStack > 0)
                {
                    int newStack = Mathf.CeilToInt(reflectStack / 2f);
                    enemyModel.enemyBuffs[reflectIdx].SetStackWithoutNotify(newStack);

                    BattleAnimSystem.Instance.QueueEnemyBuffStackPopup(reflectIdx, newStack - reflectStack);
                    BattleAnimSystem.Instance.QueueEnemyBuffStackNumChange(
                        reflectIdx, reflectStack, newStack
                    );

                    Debug.Log($"[Buff Reflect] 敌人反射从 {reflectStack} 减至 {newStack}"); 
                    ApplyPlayerDamage(DataModel<BattleModel>.Instance.PlayerModel, reflectDamage, true);
                }
            }
        }

        // 对玩家造成伤害
        public static void ApplyPlayerDamage(PlayerModel playerModel, int damage, bool fromEntity = false)
        {
            // 1. 读取“坚固”层数
            int sturdyStack = 0, sturdyIdx = -1;
            for (int i = 0; i < playerModel.Buffs.Count; i++)
            {
                if (playerModel.Buffs[i].Id == BuffIds.Sturdy)
                {
                    sturdyStack = playerModel.Buffs[i].Stack;
                    sturdyIdx = i;
                    break;
                }
            }

            // 2. 计算实际伤害
            int actualDamage = Mathf.Max(damage - sturdyStack, 0);

            // 3. 播放伤害动画
            var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
            BattleAnimSystem.Instance.QueueDamagePopup(playerView.GetHealthText().transform.position, -actualDamage);
            BattleAnimSystem.Instance.QueuePlayHealthChangeAnim(
                playerModel.Health, playerModel.Health - actualDamage
            );
            playerModel.SetHealthWithoutNotify(playerModel.Health - actualDamage);

            Debug.Log($"[Effect] 玩家失去 {actualDamage} 点生命 (原伤害{damage}，坚固抵消{sturdyStack})");
            
            //处理集中条件
            if(actualDamage > 0)
            {
                playerModel.IsFocusCondition = false;
                Debug.Log($"[Effect] Draw_Focus: 如果受伤则重置集中条件");
            }

            // 4. 如果存在坚固，就层数减半
            if (sturdyIdx >= 0 && sturdyStack > 0)
            {
                int newStack = Mathf.CeilToInt(sturdyStack / 2f);
                playerModel.Buffs[sturdyIdx].SetStackWithoutNotify(newStack);

                BattleAnimSystem.Instance.QueueBuffStackPopup(sturdyIdx, newStack - sturdyStack);
                BattleAnimSystem.Instance.QueueBuffStackNumChange(
                    sturdyIdx, sturdyStack, newStack
                );

                Debug.Log($"[Buff Sturdy] 玩家坚固从 {sturdyStack} 减至 {newStack}");
            }

            if(fromEntity)
            {
                int reflectStack = 0, reflectIdx = -1;
                for (int i = 0; i < playerModel.Buffs.Count; i++)
                {
                    if (playerModel.Buffs[i].Id == BuffIds.Reflect)
                    {
                        reflectStack = playerModel.Buffs[i].Stack;
                        reflectIdx = i;
                        break;
                    }
                }

                int reflectDamage = Mathf.Max(reflectStack, 0);

                if(reflectIdx >= 0 && reflectStack > 0)
                {
                    int newStack = Mathf.CeilToInt(reflectStack / 2f);
                    playerModel.Buffs[reflectIdx].SetStackWithoutNotify(newStack);

                    BattleAnimSystem.Instance.QueueBuffStackPopup(reflectIdx, newStack - reflectStack);
                    BattleAnimSystem.Instance.QueueBuffStackNumChange(
                        reflectIdx, reflectStack, newStack
                    );

                    Debug.Log($"[Buff Reflect] 玩家反射从 {reflectStack} 减至 {newStack}");
                    ApplyEnemyDamage(DataModel<BattleModel>.Instance.EnemyModel, reflectDamage, true);
                }
            }
        }

        // 治疗玩家
        public static void ApplyHeal(BattleModel model, int amount)
        {
            var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
            BattleAnimSystem.Instance.QueueDamagePopup(playerView.GetHealthText().transform.position, amount);
            BattleAnimSystem.Instance.QueuePlayHealthChangeAnim(model.PlayerModel.Health, model.PlayerModel.Health + amount);
            model.PlayerModel.SetHealthWithoutNotify(model.PlayerModel.Health + amount);
            Debug.Log($"[Effect] 玩家恢复 {amount} 点生命");
        }

        // 抽卡
        public static void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                BattleLogic.DrawCardToHand();
            }
            Debug.Log($"[Effect] 玩家抓了 {count} 张牌");
        }

        public static void DiscardCards(int count)
        {
            var battleModel = DataModel<BattleModel>.Instance;
            // 获取当前未被标记的卡牌索引
            var availableIndices = Enumerable.Range(0, battleModel.HandCards.Count)
                .Where(i => !BattleLogic.GetPendingRemovalIndices().Contains(i))
                .ToList();
            int discardCount = Mathf.Min(count, availableIndices.Count);
            for (int i = 0; i < discardCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
                int discardIndex = availableIndices[randomIndex];
                BattleLogic.DiscardCardFromHand(discardIndex);
                availableIndices.RemoveAt(randomIndex); // 避免重复选择
            }
            Debug.Log($"[Effect] Discard: 玩家随机弃了 {discardCount} 张牌");
        }

        public static void AddResource(BattleModel model, CardBaseType resType, int number)
        {
            CardCost res = new CardCost();
            res.SetCost(resType, number);
            BattleAnimSystem.Instance.QueueResourceBarValChange(model.PlayerModel.Resources + res, 0.2f);
            model.PlayerModel.SetResourcesWithoutNotify(model.PlayerModel.Resources + res);
        }

        #endregion

        public static void ProcessEffect(BattleModel model,CardModel cardModel, EffectIds effectId, int effectArg, int effectIndex, CardUseContext usageContext, bool conditionMet)
        {
            if (_effectHandlers.TryGetValue(effectId, out var handler))
            {
                // 直接使用预处理的条件结果
                if (conditionMet)
                    handler.Invoke(model, cardModel, effectId, effectArg, effectIndex, usageContext);
            }
            else
                Debug.LogError($"Unknown effect: {effectId}");
        }

        /// <summary>
        /// 涌动条件：玩家拥有与卡牌相同类型的元素数量 >= threshold
        /// </summary>
        private static bool HasSurgeCondition(BattleModel model, CardBaseType cardType, int threshold)
        {
            var resources = model.PlayerModel.Resources;
            switch (cardType)
            {
                case CardBaseType.Fire: return resources.Fire >= threshold;
                case CardBaseType.Water: return resources.Water >= threshold;
                case CardBaseType.Earth: return resources.Earth >= threshold;
                case CardBaseType.Air: return resources.Air >= threshold;
                default: return false;
            }
        }


        /// <summary>
        /// 检查 Effect 的 Condition 是否满足
        /// </summary>
        public static bool CheckEffectCondition(BattleModel model, CardModel cardModel, CardCondition condition, CardUseContext usageContext, int effectIndex)
        {
            switch (condition)
            {
                case CardCondition.None:
                    // None条件仅在Use场景下触发
                    return usageContext == CardUseContext.Use;
                case CardCondition.Surge:
                    if (usageContext != CardUseContext.Use)
                        return false;
                    // 涨动：玩家拥有和卡牌类型相同元素的数量不小于阈值
                    return HasSurgeCondition(model, cardModel.Id.Data().Type, model.PlayerModel.CharacterId.Data().SurgeThreshold);
                case CardCondition.Madness:
                    // 疯魔：只有在 Discard 场景下才触发
                    return usageContext == CardUseContext.Discard;
                case CardCondition.Unstable:
                    {
                        if (usageContext != CardUseContext.Use)
                            return false;

                        //如果有冷静buff，就直接触发
                        if (model.PlayerModel.CheckBuffByID(BuffIds.Calm))
                        {
                            return true;
                        }
                        // 不稳定：50%概率
                        return UnityEngine.Random.value < 0.5f;
                    }
                case CardCondition.Cooldown:
                    if (usageContext != CardUseContext.Use)
                        return false;
                    //若自身处于燃烧（Burn)状态，则触发，然后清除自身所有燃烧状态
                    return model.PlayerModel.CheckBuffByID(BuffIds.Burning); ;
                case CardCondition.Peace:
                    if (usageContext != CardUseContext.Use)
                        return false;
                    //若没有手牌(出的这张是最后一张手牌），则触发
                    return model.HandCards.Count == 1;
                case CardCondition.Focus:
                    //若此次行动没有受到伤害，则触发，需要延迟结算
                    return true;
                default:
                    Debug.LogWarning($"未知的 CardCondition: {condition}");
                    return false;
            }
        }
    }
}