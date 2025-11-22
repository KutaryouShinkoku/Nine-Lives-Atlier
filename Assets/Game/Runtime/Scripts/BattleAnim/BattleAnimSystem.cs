using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.UI.InGame;
using UnityEngine;
using UnityEngine.Serialization;
using WanFramework.Base;
using WanFramework.UI;

namespace Game.BattleAnim
{
    [SystemPriority(SystemPriorities.UI - 1)]
    public partial class BattleAnimSystem : SystemBase<BattleAnimSystem>
    {
        [SerializeField]
        private Queue<BattleAnim> battleAnimQueue = new();

        private InGamePlayerView _playerView;
        private InGameEnemyView _enemyView;
        private InGameHandAreaView _handAreaView;
        private UIPopupNumberView _popupNumberView;
        private void QueueAnim(BattleAnim anim) => battleAnimQueue.Enqueue(anim);
        public void ClearAnimQueue() => battleAnimQueue.Clear();
        public async UniTask<bool> TryDequeAndPlayAnim(CancellationToken token)
        {
            if (!battleAnimQueue.TryDequeue(out var anim)) return false;
            await PlayAnim(anim, token);
            return true;
        }

        public override UniTask Init()
        {
            _playerView = UISystem.Instance.GetCommonView<InGamePlayerView>("InGame/Player");
            _enemyView = UISystem.Instance.GetCommonView<InGameEnemyView>("InGame/Enemy");
            _handAreaView = UISystem.Instance.GetCommonView<InGameHandAreaView>("InGame/HandAreaView");
            _popupNumberView = UISystem.Instance.ShowUI<UIPopupNumberView>("InGame/UIPopupNumber");
            return base.Init();
        }

        public async UniTask PlayAllQueuedAnim(CancellationToken token)
        {
            while (battleAnimQueue.Count != 0)
                await PlayAnim(battleAnimQueue.Dequeue(), token);
            battleAnimQueue.Clear();
            await UniTask.NextFrame(token);
        }
        
        /// <summary>
        /// 不等待指定task，并返回CompletedTask
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private UniTask Forget(UniTask task)
        {
            task.Forget();
            return UniTask.CompletedTask;
        }
        
        private UniTask PlayAnim(BattleAnim anim, CancellationToken token)
        {
            /*#if UNITY_EDITOR
            Debug.Log($"Play {anim.animType}");
            #endif*/
            return anim.animType switch
            {
                BattleAnimType.DrawCardToHand => PlayDrawCardToHandAnim(token, anim),
                BattleAnimType.SacrificeCardFromHand => PlaySacrificeCardFromHandAnim(token, anim),
                BattleAnimType.UseCardFromHand => PlayUseCardFromHandAnim(token, anim),
                BattleAnimType.DiscardCard => PlayDiscardCardAnim(token, anim),
                BattleAnimType.PlayerAttackEnemy => PlayPlayerAttackEnemyAnim(token, anim),
                BattleAnimType.EnemyAttackPlayer => PlayEnemyAttackPlayerAnim(token, anim),
                BattleAnimType.SetCardVisibility => PlaySetCardVisibility(token, anim),

                //敌人受伤动画不用等待
                BattleAnimType.EnemyTakeDamage => Forget(PlayEnemyTakeDamageAnim(token, anim)),
                BattleAnimType.EnemyTakeBuffDamage => Forget(PlayEnemyTakeBuffDamageAnim(token, anim)),

                BattleAnimType.MoveCardToEnemy => PlayMoveCardToEnemyAnim(token, anim.animArgI0),
                BattleAnimType.MoveCardToResource => PlayMoveCardToResourceAnim(token, anim.animArgI0),
                
                BattleAnimType.ShakeCamera => Forget(PlayShakeCameraAnim(token, anim.animArgF0)),

                BattleAnimType.HealthChange => PlayHealthChangeAnim(token, _playerView.GetHealthText(), anim.animArgI0, anim.animArgI1),
                BattleAnimType.EnemyHealthChange => PlayEnemyHealthChangeAnim(token, _enemyView.GetHealthText(), anim.animArgI0, anim.animArgI1),
                BattleAnimType.EnemyAttackChange => PlayEnemyAttackChangeAnim(token, _enemyView.GetAttackText(), anim.animArgI0, anim.animArgI1),
                BattleAnimType.EnemyRewardChange => PlayEnemyRewardChangeAnim(token, _enemyView.GetRewardText(), anim.animArgI0, anim.animArgI1),

                //BUFF
                BattleAnimType.BuffStackAdd => PlayBuffStackAddAnim(token, anim.animArgI0),
                BattleAnimType.EnemyBuffStackAdd => PlayEnemyBuffStackAddAnim(token, anim.animArgI0),
                BattleAnimType.BuffStackMinus => PlayBuffStackMinusAnim(token, (int)anim.animArgF0),
                BattleAnimType.EnemyBuffStackMinus => PlayEnemyBuffStackMinusAnim(token, (int)anim.animArgF0),
                BattleAnimType.BuffEffect => PlayBuffEffectAnim(token, anim.animArgI0),
                BattleAnimType.EnemyBuffEffect => PlayEnemyBuffEffectAnim(token, anim.animArgI0),

                BattleAnimType.BuffShow => PlayBuffShowAnim(token, anim.animArgI0),
                BattleAnimType.EnemyBuffShow => PlayEnemyBuffShowAnim(token, anim.animArgI0),

                BattleAnimType.BuffStackPopup => Forget(PlayBuffStackPopupAnim(token, anim)),
                BattleAnimType.EnemyBuffStackPopup => Forget(PlayEnemyBuffStackPopupAnim(token, anim)),
                BattleAnimType.BuffStackNumChange => Forget(PlayBuffStackNumChangeAnim(token, anim)),
                BattleAnimType.EnemyBuffStackNumChange => Forget(PlayEnemyBuffStackNumChangeAnim(token, anim)),

                //弹数字
                BattleAnimType.DamagePopup => Forget(PlayDamagePopupAnim(token, anim)),
                BattleAnimType.BuffPopup => Forget(PlayBuffPopupAnim(token, anim)),

                BattleAnimType.ResourceBarValChange => PlayResourceBarValChangeAnim(token, anim.animArgCardCost, anim.animArgF0),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}