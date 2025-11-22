using System;
using System.Buffers;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.BattleAnim;
using Game.Data;
using Game.Model;
using Game.Model.InGameSubModel;
using Game.UI.InGame;
using UnityEngine;
using WanFramework.UI;
using WanFramework.UI.DataComponent;

namespace Game.Tutorial
{
    public partial class TutorialRunner
    {
        private async UniTask HandleAction(CancellationToken token, TutorialAction action, string[] args)
        {
            switch (action)
            {
                case TutorialAction.None:
                    break;
                case TutorialAction.DrawCard:
                    if (args.Length == 0)
                        Debug.LogError("DrawCard需要1个参数");
                    else if (!Enum.TryParse<CardIds>(args[0], out var drawCard))
                        Debug.LogError($"{args[0]}不是个有效的卡牌Id");
                    else
                    {
                        DataModel<BattleModel>.Instance.HandCards.Add(new CardModel
                        {
                            Id = drawCard
                        });
                        await BattleAnimSystem.Instance.PlayDrawCardToHandAnim(token, DataModel<BattleModel>.Instance.HandCards.Count - 1);
                    }
                    break;
                case TutorialAction.SacrificeLastCard:
                    if (_lastSacrificeCardIndex == -1)
                        Debug.LogError("玩家未曾献祭过卡牌");
                    else
                    {
                        await BattleAnimSystem.Instance.PlayMoveCardToResourceAnim(token, _lastSacrificeCardIndex);
                        await BattleAnimSystem.Instance.PlaySacrificeCardFromHandAnim(token, _lastSacrificeCardIndex);
                        await UniTask.WaitForSeconds(0.5f, cancellationToken: token);
                        DataModel<BattleModel>.Instance.HandCards.RemoveAt(_lastSacrificeCardIndex);
                    }
                    break;
                case TutorialAction.UseLastCard:
                    if (_lastUseCardIndex == -1)
                        Debug.LogError("玩家未曾用过卡牌");
                    else
                    {
                        await BattleAnimSystem.Instance.PlayMoveCardToEnemyAnim(token, _lastUseCardIndex);
                        await BattleAnimSystem.Instance.PlayUseCardFromHandAnim(token, _lastUseCardIndex);
                        await UniTask.WaitForSeconds(0.5f, cancellationToken: token);
                        DataModel<BattleModel>.Instance.HandCards.RemoveAt(_lastUseCardIndex);
                    }
                    break;
                case TutorialAction.SacrificeHandCard:
                    if (args.Length == 0)
                        Debug.LogError("献祭手牌需要1个参数作来选择献祭牌Index");
                    else
                    {
                        await BattleAnimSystem.Instance.PlayMoveCardToResourceAnim(token, int.Parse(args[0]));
                        await BattleAnimSystem.Instance.PlaySacrificeCardFromHandAnim(token, int.Parse(args[0]));
                        await UniTask.WaitForSeconds(0.5f, cancellationToken: token);
                        DataModel<BattleModel>.Instance.HandCards.RemoveAt(int.Parse(args[0]));
                    }
                    break;
                case TutorialAction.UseHandCard:
                    if (args.Length == 0)
                        Debug.LogError("使用手牌需要1个参数作为使用卡牌Index");
                    else
                    {
                        await BattleAnimSystem.Instance.PlayMoveCardToEnemyAnim(token, int.Parse(args[0]));
                        await BattleAnimSystem.Instance.PlayUseCardFromHandAnim(token, int.Parse(args[0]));
                        DataModel<BattleModel>.Instance.HandCards.RemoveAt(int.Parse(args[0]));
                    }
                    break;
                case TutorialAction.PlayEnemyAttack:
                    if (args.Length != 1)
                        Debug.LogError("播放敌人攻击需要1个参数");
                    else
                    {
                        await BattleAnimSystem.Instance.PlayEnemyAttackPlayerAnim(token);
                        DataModel<BattleModel>.Instance.PlayerModel.Health -= int.Parse(args[0]);
                    }
                    break;
                case TutorialAction.PlayPlayerAttack:
                    if (args.Length != 1)
                        Debug.LogError("播放玩家人攻击需要1个参数");
                    else
                    {
                        await BattleAnimSystem.Instance.PlayPlayerAttackEnemyAnim(token);
                        await BattleAnimSystem.Instance.PlayeEnemyTakeDamageAnim(token);
                        DataModel<BattleModel>.Instance.EnemyModel.Health -= int.Parse(args[0]);
                    }
                    break;
                case TutorialAction.IncreaseResource:
                    if (args.Length != 4)
                        Debug.LogError("增加资源需要4个参数");
                    else
                    {
                        var pm = DataModel<BattleModel>.Instance.PlayerModel;
                        var res = pm.Resources;
                        res.Air += int.Parse(args[(int)CardBaseType.Air]);
                        res.Fire += int.Parse(args[(int)CardBaseType.Fire]);
                        res.Earth += int.Parse(args[(int)CardBaseType.Earth]);
                        res.Water += int.Parse(args[(int)CardBaseType.Water]);
                        await BattleAnimSystem.Instance.PlayResourceBarValChangeAnim(token, res, 0.5f);
                        pm.Resources = res;
                    }
                    break;
                case TutorialAction.SetResource:
                    if (args.Length != 4)
                        Debug.LogError("设置资源需要4个参数");
                    else
                    {
                        var pm = DataModel<BattleModel>.Instance.PlayerModel;
                        var res = new CardCost
                        {
                            Air = int.Parse(args[(int)CardBaseType.Air]),
                            Fire = int.Parse(args[(int)CardBaseType.Fire]),
                            Earth = int.Parse(args[(int)CardBaseType.Earth]),
                            Water = int.Parse(args[(int)CardBaseType.Water])
                        };
                        pm.Resources = res;
                    }
                    break;
                case TutorialAction.SetCharacter:
                    if (args.Length != 1)
                        Debug.LogError("设置角色需要1个参数");
                    else if (!Enum.TryParse<CharacterIds>(args[0], out var characterId))
                        Debug.LogError($"{args[0]}不是有效的角色Id");
                    else
                        DataModel<BattleModel>.Instance.PlayerModel.CharacterId = characterId;
                    break;
                case TutorialAction.SetHealth:
                    if (args.Length != 1)
                        Debug.LogError("设置角色生命值需要1个参数");
                    else
                        DataModel<BattleModel>.Instance.PlayerModel.Health = int.Parse(args[0]);
                    break;
                case TutorialAction.SetEnemy:
                    if (args.Length != 1)
                        Debug.LogError("设置敌人需要1个参数");
                    else if (!Enum.TryParse<EnemyIds>(args[0], out var enemyId))
                        Debug.LogError($"{args[0]}不是有效的敌人色Id");
                    else
                        DataModel<BattleModel>.Instance.EnemyModel.EnemyId = enemyId;
                    break;
                case TutorialAction.SetEnemyHealth:
                    if (args.Length != 1)
                        Debug.LogError("设置敌人生命值需要1个参数");
                    else
                        DataModel<BattleModel>.Instance.EnemyModel.Health = int.Parse(args[0]);
                    break;
                case TutorialAction.SetEnemyAttack:
                    if (args.Length != 1)
                        Debug.LogError("设置敌人攻击需要1个参数");
                    else
                        DataModel<BattleModel>.Instance.EnemyModel.Attack = int.Parse(args[0]);
                    break;
                case TutorialAction.SetEnemyReward:
                    if (args.Length != 1)
                        Debug.LogError("设置敌人奖励值需要1个参数");
                    else
                        DataModel<BattleModel>.Instance.EnemyModel.Reward = int.Parse(args[0]);
                    break;
                case TutorialAction.EnableDrag:
                    var handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.CanDragCard = true;
                    break;
                case TutorialAction.DisableDrag:
                    handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.CanDragCard = false;
                    break;
                case TutorialAction.EnableDraw:
                    handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.CanDrawCard = true;
                    break;
                case TutorialAction.DisableDraw:
                    handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.CanDrawCard = false;
                    break;
                case TutorialAction.EnableSelect:
                    handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.CanSelectCard = true;
                    break;
                case TutorialAction.DisableSelect:
                    handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
                    handAreaView.CanSelectCard = false;
                    break;
                case TutorialAction.HealthChange:
                    if (args.Length != 2)
                        Debug.LogError("设置玩家生命值变化需要2个参数args[0]为变化前，args[1]为变化后");
                    else
                    {
                        var _playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
                        BattleAnimSystem.Instance.PlayDamagePopupAnimWithPos(token, _playerView.GetHealthText().transform.position, (int.Parse(args[1]) - int.Parse(args[0]))).Forget();
                        await BattleAnimSystem.Instance.PlayHealthChangeAnim(token, _playerView.GetHealthText(), int.Parse(args[0]), int.Parse(args[1]));
                    }
                    break;
                case TutorialAction.BuffStackChange:
                    if (args.Length != 3)
                        Debug.LogError("设置玩家BuffStack变化需要3个参数(args[0]指定第几个buff,args[1]为变化前，args[2]为变化后)");
                    else
                    {
                        var playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
                        await BattleAnimSystem.Instance.PlayBuffStackMinusAnim(token, int.Parse(args[0]));
                    }
                    break;

                case TutorialAction.EnemyBuffStackChange:
                    if (args.Length != 3)
                        Debug.LogError("设置敌方BuffStack变化需要3个参数(args[0]指定第几个buff,args[1]为变化前，args[2]为变化后)");
                    else
                    {
                        var _enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
                        await BattleAnimSystem.Instance.PlayEnemyBuffStackMinusAnim(token, int.Parse(args[0]));
                    }
                    break;
                case TutorialAction.EnemyHealthChange:
                    if (args.Length != 2)
                        Debug.LogError("设置敌方生命值变化需要2个参数args[0]为变化前，args[1]为变化后");
                    else
                    {
                        var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
                        var newVal = int.Parse(args[1]);
                        var oldVal = int.Parse(args[0]);
                        BattleAnimSystem.Instance.PlayDamagePopupAnimWithPos(token, enemyView.GetHealthText().transform.position, newVal - oldVal).Forget();
                        await BattleAnimSystem.Instance.PlayEnemyHealthChangeAnim(token, enemyView.GetHealthText(), oldVal, newVal);
                        DataModel<BattleModel>.Instance.EnemyModel.Health = newVal;
                    }
                    break;
                case TutorialAction.EnemyAttackChange:
                    if (args.Length != 2)
                        Debug.LogError("设置敌方攻击值变化需要2个参数args[0]为变化前，args[1]为变化后");
                    else
                    {
                        var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
                        var newVal = int.Parse(args[1]);
                        var oldVal = int.Parse(args[0]);
                        BattleAnimSystem.Instance.PlayDamagePopupAnimWithPos(token, enemyView.GetAttackText().transform.position, newVal - oldVal).Forget();
                        await BattleAnimSystem.Instance.PlayEnemyAttackChangeAnim(token, enemyView.GetAttackText(), oldVal, newVal);
                        DataModel<BattleModel>.Instance.EnemyModel.Attack = newVal;
                    }
                    break;
                case TutorialAction.EnemyRewardChange:
                    if (args.Length != 2)
                        Debug.LogError("设置敌方奖励值变化需要2个参数args[0]为变化前，args[1]为变化后");
                    else
                    {
                        var enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
                        var newVal = int.Parse(args[1]);
                        var oldVal = int.Parse(args[0]);
                        BattleAnimSystem.Instance.PlayDamagePopupAnimWithPos(token, enemyView.GetRewardText().transform.position, newVal - oldVal).Forget();
                        await BattleAnimSystem.Instance.PlayEnemyRewardChangeAnim(token, enemyView.GetRewardText(), oldVal, newVal);
                        DataModel<BattleModel>.Instance.EnemyModel.Reward = newVal;
                    }
                    break;
                case TutorialAction.Conditional:
                    if (args.Length == 0)
                        Debug.LogError("条件Action需要指定至少1个跳转目标");
                    var conditions = ArrayPool<(TutorialTable.Entry Data, TutorialIds Id)>.Shared.Rent(args.Length);
                    for (var i = 0; i < args.Length; i++)
                    {
                        if (!Enum.TryParse<TutorialIds>(args[i], out var tutorialId))
                            Debug.LogError($"{args[i]} 不是一个有效的TutorialIds");
                        conditions[i] = (tutorialId.Data(), tutorialId);
                    }
                    var hasAnySatisfied = false;
                    while (!hasAnySatisfied)
                    {
                        foreach (var condition in conditions)
                        {
                            if (!HandleCondition(condition.Data.Condition, condition.Data.ConditionArgs)) continue;
                            _next = condition.Id + 1;
                            hasAnySatisfied = true;
                            break;
                        }
                        await UniTask.NextFrame(token);
                    }
                    ArrayPool<(TutorialTable.Entry Data, TutorialIds Id)>.Shared.Return(conditions);
                    break;
                case TutorialAction.Goto:
                    if (args.Length != 1)
                        Debug.LogError("设置跳转需要指定目标的Id");
                    if (!Enum.TryParse<TutorialIds>(args[0], out var gotoNext))
                        Debug.LogError($"{args[0]} 不是一个有效的TutorialIds");
                    _next = gotoNext;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
    }
}