using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.UI.Common.Components;
using Game.UI.InGame;
using Game.Audio;
using Game.Data;
using Game.UI.InGame.Components;
using Game.Utils;
using TMPro;
using UnityEngine;
using WanFramework.Base;
using WanFramework.UI;

namespace Game.BattleAnim
{
    public partial class BattleAnimSystem : SystemBase<BattleAnimSystem>
    {
        #region ResourceBarAnim

        public void QueueResourceBarValChange(CardCost res, float time)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.ResourceBarValChange,
                animArgCardCost = res,
                animArgF0 = time
            });
        }

        public async UniTask PlayResourceBarValChangeAnim(CancellationToken token, CardCost res, float time)
        {
            _playerView.PlayResourceBarValChangeAnim(res);
            await UniTask.WaitForSeconds(time, cancellationToken: token);
        }

        public async UniTask PlayGaugesValueChangeAnim(int value,float time)
        {
            //如果需要更精细的动画，在此调整
            _playerView.PlayGaugesValueChangeAnim(value);
            await UniTask.WaitForSeconds(time);
        }

        #endregion

        #region PopupNumber Animations

        public void QueueBuffStackPopup(int buffIndex, int amount)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffStackPopup,
                animArgI0 = buffIndex,
                animArgI1 = amount
            });
        }

        public void QueueEnemyBuffStackPopup(int buffIndex, int amount)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.EnemyBuffStackPopup,
                animArgI0 = buffIndex,
                animArgI1 = amount
            });
        }

        private async UniTask PlayBuffStackPopupAnim(CancellationToken token, BattleAnim anim)
        {
            var target = _playerView.GetBuffCollectionView().GetSubView(anim.animArgI0) as CommonUIBuffView;
            var pos = target.transform.position;
            var amount = anim.animArgI1;
            await _popupNumberView.ProcessDamage(pos, amount, token);
        }

        private async UniTask PlayEnemyBuffStackPopupAnim(CancellationToken token, BattleAnim anim)
        {
            var target = _enemyView.GetBuffCollectionView().GetSubView(anim.animArgI0) as CommonUIBuffView;
            var pos = target.transform.position;
            var amount = anim.animArgI1;
            await _popupNumberView.ProcessDamage(pos, amount, token);
        }

        public void QueueDamagePopup(Vector3 worldPos, int amount)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.DamagePopup,
                animArgI0 = amount,
                animArgF0 = worldPos.x,
                animArgF1 = worldPos.y,
                animArgF2 = worldPos.z
            });
        }

        public void QueueBuffPopup(Vector3 worldPos, Sprite icon)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffPopup,
                animArgF0 = worldPos.x,
                animArgF1 = worldPos.y,
                animArgF2 = worldPos.z,
                animArgObj0 = icon
            });
        }

        public async UniTask PlayDamagePopupAnimWithPos(CancellationToken token, Vector3 position, int amount)
        {
            await _popupNumberView.ProcessDamage(position, amount, token);
        }

        public async UniTask PlayBuffPopupAnimpWithPos(CancellationToken token, Vector3 position, Sprite icon)
        {
            await _popupNumberView.ProcessBuff(position, icon, token);
        }

        private async UniTask PlayDamagePopupAnim(CancellationToken token, BattleAnim anim)
        {
            var pos = new Vector3(anim.animArgF0, anim.animArgF1, anim.animArgF2);
            var amount = anim.animArgI0;
            await _popupNumberView.ProcessDamage(pos, amount, token);
        }

        private async UniTask PlayBuffPopupAnim(CancellationToken token, BattleAnim anim)
        {
            var pos = new Vector3(anim.animArgF0, anim.animArgF1, anim.animArgF2);
            var icon = (Sprite)anim.animArgObj0;
            await _popupNumberView.ProcessBuff(pos, icon, token);
        }

        #endregion

        #region NumberChangeAnim

        public void QueueBuffStackNumChange(int buffIndex, int oldVal, int newVal)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffStackNumChange,
                animArgI0 = buffIndex,
                animArgI1 = oldVal,
                animArgI2 = newVal
            });
        }

        public void QueueEnemyBuffStackNumChange(int buffIndex, int oldVal, int newVal)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.EnemyBuffStackNumChange,
                animArgI0 = buffIndex,
                animArgI1 = oldVal,
                animArgI2 = newVal
            });
        }

        private async UniTask PlayBuffStackNumChangeAnim(CancellationToken token, BattleAnim anim)
        {
            var target = _playerView.GetBuffCollectionView().GetSubView(anim.animArgI0) as CommonUIBuffView;
            await PlayNumberChangeAnim(token, target.TextStack, anim.animArgI1, anim.animArgI2);
            target.SetStack(anim.animArgI2);
        }
        
        public async UniTask PlayEnemyBuffStackNumChangeAnim(CancellationToken token, BattleAnim anim)
        {
            var target = _enemyView.GetBuffCollectionView().GetSubView(anim.animArgI0) as CommonUIBuffView;
            await PlayNumberChangeAnim(token, target.TextStack, anim.animArgI1, anim.animArgI2);
            target.SetStack(anim.animArgI2);
        }

        /// <summary>
        /// 玩家生命值变化动画
        /// </summary>
        public void QueuePlayHealthChangeAnim(int oldHealth, int newHealth)
        {
            if (_playerView != null && _playerView.GetHealthText() != null)
            {
                QueueAnim(new BattleAnim
                {
                    animType = BattleAnimType.HealthChange,
                    animArgI0 = oldHealth,
                    animArgI1 = newHealth
                });
            }
            else
            {
                Debug.LogWarning("PlayerView or its health text is null, cannot queue HealthChangeAnim.");
            }
        }

        /// <summary>
        /// 敌人生命值变化动画
        /// </summary>
        public void QueuePlayEnemyHealthChangeAnim(int oldHealth, int newHealth)
        {
            if (_enemyView != null && _enemyView.GetHealthText() != null)
            {
                
                QueueAnim(new BattleAnim
                {
                    animType = BattleAnimType.EnemyHealthChange,
                    animArgI0 = oldHealth,
                    animArgI1 = newHealth
                });
            }
            else
            {
                Debug.LogWarning("EnemyView or its health text is null, cannot queue EnemyHealthChangeAnim.");
            }
        }

        /// <summary>
        /// 敌人攻击值变化动画
        /// </summary>
        public void QueueEnemyAttackChangeAnim(int oldAttack, int newAttack)
        {
            if (_enemyView != null && _enemyView.GetAttackText() != null)
            {
                
                QueueAnim(new BattleAnim
                {
                    animType = BattleAnimType.EnemyAttackChange,
                    animArgI0 = oldAttack,
                    animArgI1 = newAttack
                });
            }
            else
            {
                Debug.LogWarning("EnemyView or its attack text is null, cannot queue EnemyAttackChangeAnim.");
            }
        }

        /// <summary>
        /// 敌人奖励值变化动画
        /// </summary>
        public void QueueEnemyRewardChangeAnim(int oldReward, int newReward)
        {
            if (_enemyView != null && _enemyView.GetRewardText() != null)
            {
                
                QueueAnim(new BattleAnim
                {
                    animType = BattleAnimType.EnemyRewardChange,
                    animArgI0 = oldReward,
                    animArgI1 = newReward
                });
            }
            else
            {
                Debug.LogWarning("EnemyView or its reward text is null, cannot queue EnemyRewardChangeAnim.");
            }
        }


        /// <summary>
        /// 通用数字变化动画。
        /// 使用传入的 TMP_Text 对象，从 oldValue 渐变到 newValue，
        /// 同时对文本进行缩放和轻微抖动效果，整个动画时长为 duration 毫秒。
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <param name="target">目标文本组件</param>
        /// <param name="oldValue">旧数字</param>
        /// <param name="newValue">新数字</param>
        /// <param name="duration">总动画时长，单位为毫秒</param>
        /// <param name="jumpScale">缩放幅度（例如 0.2 表示 20%）</param>
        /// <param name="shakeRange">抖动范围（例如 2f）</param>
        public static async UniTask PlayNumberChangeAnim(
            CancellationToken token,
            TMP_Text target,
            int oldValue,
            int newValue,
            float duration = 300f,
            float jumpScale = 0.2f,
            float shakeRange = 2f)
        {
            int steps = 6; // 分成6步变化
            float delay = duration / steps;
            int diff = newValue - oldValue;
            Vector3 originalScale = target.transform.localScale;
            Vector3 originalPos = target.transform.localPosition;
            try
            {
                for (int i = 1; i <= steps; i++)
                {
                    if (token.IsCancellationRequested)
                        break;
                    int currentValue = oldValue + (int)(diff * ((float)i / steps));
                    target.SetText(currentValue.ToString());
                    // 利用正弦函数实现数字跳动效果
                    float scaleFactor = 1 + jumpScale * Mathf.Sin(((float)i / steps) * Mathf.PI);
                    target.transform.localScale = originalScale * scaleFactor;
                    // 随机轻微抖动
                    float offsetX = UnityEngine.Random.Range(-shakeRange, shakeRange);
                    float offsetY = UnityEngine.Random.Range(-shakeRange, shakeRange);
                    target.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
                    await UniTask.Delay((int)delay, cancellationToken: token, cancelImmediately: true);
                }
            }
            finally
            {
                // 即使cancel了，也设置一下大小
                target.SetText(newValue.ToString());
                target.transform.localScale = originalScale;
                target.transform.localPosition = originalPos;
            }
        }

        public async UniTask PlayHealthChangeAnim(CancellationToken token, TMP_Text healthText, int oldHealth, int newHealth)
        {
            if (oldHealth < newHealth) { AudioSystem.Instance.SendEvent(AudioIds.Effect_Player_Heal); }
            else { AudioSystem.Instance.SendEvent(AudioIds.Effect_Player_Damaged); }
            await PlayNumberChangeAnim(token, healthText, oldHealth, newHealth);
        }

        public async UniTask PlayEnemyHealthChangeAnim(CancellationToken token, TMP_Text healthText, int oldHealth, int newHealth)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Enemy_Health_Change);
            await PlayNumberChangeAnim(token, healthText, oldHealth, newHealth);
        }

        public async UniTask PlayEnemyAttackChangeAnim(CancellationToken token, TMP_Text attackText, int oldAttack, int newAttack)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Enemy_Attack_Change);
            await PlayNumberChangeAnim(token, attackText, oldAttack, newAttack);
        }

        public async UniTask PlayEnemyRewardChangeAnim(CancellationToken token, TMP_Text rewardText, int oldReward, int newReward)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Enemy_Reward_Change);
            await PlayNumberChangeAnim(token, rewardText, oldReward, newReward);
        }


        #endregion

        #region 晃动相机
        public void QueueShakeCameraAnim(float time) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.ShakeCamera,
            animArgF0 = time
        });
        public async UniTask PlayShakeCameraAnim(CancellationToken token, float time)
        {
            if (GameManager.Current.MainCamera.TryGetComponent(out Shaker shaker))
                shaker.Shake(time);
            else
                Debug.LogWarning("Shaker is null, cannot play ShakeCameraAnim on Main Camera.");
            await UniTask.WaitForSeconds(time, cancellationToken: token);
        }
        #endregion
        
        #region 移动卡牌到目标位置
        public void QueueMoveCardToEnemyAnim(int handIndex) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.MoveCardToEnemy,
            animArgI0 = handIndex
        });
        public UniTask PlayMoveCardToEnemyAnim(CancellationToken token, int handIndex)
        {
            _handAreaView.HandlingUseCard(handIndex);
            var targetPos = new Vector3(4.4f, 10f, 0);
            _handAreaView.SetTargetPosition(handIndex, targetPos);
            return UniTask.CompletedTask;
        }
        public void QueueMoveCardToResourceAnim(int handIndex) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.MoveCardToResource,
            animArgI0 = handIndex
        });
        public UniTask PlayMoveCardToResourceAnim(CancellationToken token, int handIndex)
        {
            _handAreaView.HandlingUseCard(handIndex);
            var targetPos = new Vector3(93f, 40f, 0);
            _handAreaView.SetTargetPosition(handIndex, targetPos);
            return UniTask.CompletedTask;
        }
        #endregion

        #region BuffShowAnim

        public void QueueBuffShowAnim(int buffIndex)
        {
            var target = _playerView.GetBuffCollectionView().GetSubView(buffIndex) as CommonUIBuffView;
            target.gameObject.SetActive(false);
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffShow,
                animArgI0 = buffIndex
            });
        }

        public void QueueEnemyBuffShowAnim(int buffIndex)
        {
            var target = _enemyView.GetBuffCollectionView().GetSubView(buffIndex) as CommonUIBuffView;
            target.gameObject.SetActive(false);
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.EnemyBuffShow,
                animArgI0 = buffIndex
            });
        }


        public async UniTask PlayBuffShowAnim(CancellationToken token, int buffIndex)
        {
            var target = _playerView.GetBuffCollectionView().GetSubView(buffIndex) as CommonUIBuffView;
            if (target == null) return;
            target.gameObject.SetActive(true);
            target.RaiseEventOnStart = false;
            target.SetStack(0);
            var buffShowAnim = target.PlayBuffShow(token);
            await buffShowAnim;
        }

        public async UniTask PlayEnemyBuffShowAnim(CancellationToken token, int buffIndex)
        {
            var target = _enemyView.GetBuffCollectionView().GetSubView(buffIndex) as CommonUIBuffView;
            if (target == null) return;
            target.gameObject.SetActive(true);
            target.RaiseEventOnStart = false;
            target.SetStack(0);
            var buffShowAnim = target.PlayBuffShow(token);
            await buffShowAnim;
        }

        #endregion

        #region BuffStackChangeAnim

        //Buff增加
        public void QueueBuffStackAddAnim(int buffIndex)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffStackAdd,
                animArgI0 = buffIndex,
            });
        }

        public void QueueEnemyBuffStackAddAnim(int buffIndex)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.EnemyBuffStackAdd,
                animArgI0 = buffIndex,
            });
        }


        public async UniTask PlayBuffStackAddAnim(CancellationToken token, int buffIndex)
        {
            var target = _playerView.GetBuffCollectionView().GetSubView(buffIndex) as CommonUIBuffView;
            if (target == null) return;
            var buffAddAnim = target.PlayBuffStackAddAnim(token);
            await buffAddAnim;
        }

        public async UniTask PlayEnemyBuffStackAddAnim(CancellationToken token, int buffIndex)
        {
            var target = _enemyView.GetBuffCollectionView().GetSubView(buffIndex) as CommonUIBuffView;
            if (target == null) return;
            var buffAddAnim = target.PlayBuffStackAddAnim(token);
            await buffAddAnim;
        }

        //Buff减少
        public void QueueBuffStackMinusAnim(int index)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffStackMinus,
                animArgF0 = index
            });
        }

        public void QueueEnemyBuffStackMinusAnim(int index)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.EnemyBuffStackMinus,
                animArgF0 = index
            });
        }

        public async UniTask PlayBuffStackMinusAnim(CancellationToken token, int index)
        {
            CommonUIBuffView target = _playerView.GetBuffCollectionView().GetSubView(index) as CommonUIBuffView;
            if (target == null) return;
            var buffMinusAnim = target.PlayBuffStackMinusAnim(token);
            await buffMinusAnim;
        }

        public async UniTask PlayEnemyBuffStackMinusAnim(CancellationToken token, int index)
        {
            CommonUIBuffView target = _enemyView.GetBuffCollectionView().GetSubView(index) as CommonUIBuffView;
            if (target == null) return;
            var buffMinusAnim = target.PlayBuffStackMinusAnim(token);
            await buffMinusAnim;
        }

        //Buff生效
        public void QueueBuffEffect(int index)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.BuffEffect,
                animArgI0 = index
            });
        }

        public void QueueEnemyBuffEffect(int index)
        {
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.EnemyBuffEffect,
                animArgI0 = index
            });
        }

        public async UniTask PlayBuffEffectAnim(CancellationToken token, int index)
        {
            Debug.Log($"[ANIM] 播放玩家 Buff生效动画");
            CommonUIBuffView target = _playerView.GetBuffCollectionView().GetSubView(index) as CommonUIBuffView;
            if (target == null) return;
            var buffEffect = target.PlayBuffEffect(token);
            await buffEffect;
        }

        public async UniTask PlayEnemyBuffEffectAnim(CancellationToken token, int index)
        {
            Debug.Log($"[ANIM] 播放敌人 Buff生效动画");
            CommonUIBuffView target = _enemyView.GetBuffCollectionView().GetSubView(index) as CommonUIBuffView;
            if (target == null) return;
            var buffEffect = target.PlayBuffEffect(token);
            await buffEffect;
        }
        #endregion

        #region DrawCardToHandAnim
        public void QueueDrawCardToHandAnim(int handIndex)
        {
            var handAreaView = UISystem.Instance.ShowCommonView<InGameHandAreaView>("InGame/HandAreaView");
            handAreaView.GetCardView(handIndex).IsVisible = false;
            QueueAnim(new BattleAnim
            {
                animType = BattleAnimType.DrawCardToHand,
                animArgI0 = handIndex
            });
        }
        public async UniTask PlayDrawCardToHandAnim(CancellationToken token, int handIndex)
        {
            AudioSystem.Instance.SendEvent(AudioIds.Effect_Card_Draw);
            var drawCardView = _handAreaView.GetCardView(handIndex);
            drawCardView.IsVisible = true;
            await drawCardView.PlayShowAnim(token);
        }
        private UniTask PlayDrawCardToHandAnim(CancellationToken token, in BattleAnim anim) => PlayDrawCardToHandAnim(token, anim.animArgI0);
        #endregion
        
        #region UseCardFromHandAnim
        public void QueueUseCardFromHandAnim(int handIndex) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.UseCardFromHand,
            animArgI0 = handIndex
        });
        public async UniTask PlayUseCardFromHandAnim(CancellationToken token, int handIndex)
        {
            var drawCardView = _handAreaView.GetCardView(handIndex);
            drawCardView.IsVisible = true;
            await drawCardView.PlayUseCardAnim(token);
        }
        private UniTask PlayUseCardFromHandAnim(CancellationToken token, in BattleAnim anim) => PlayUseCardFromHandAnim(token, anim.animArgI0);
        #endregion

        #region DiscardCardAnim
        public void QueueDiscardCardAnim(int handIndex) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.DiscardCard,
            animArgI0 = handIndex
        });
        public async UniTask PlayDiscardCardAnim(CancellationToken token, int handIndex)
        {
            var drawCardView = _handAreaView.GetCardView(handIndex);
            drawCardView.IsVisible = true;
            await drawCardView.PlayDiscardAnim(token);
        }
        private UniTask PlayDiscardCardAnim(CancellationToken token, in BattleAnim anim) => PlayDiscardCardAnim(token, anim.animArgI0);
        #endregion

        #region SetCardVisibility
        public void QueueSetCardVisibility(int handIndex, bool isVisible) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.SetCardVisibility,
            animArgI0 = handIndex,
            animArgI1 = isVisible ? 1 : 0
        });
        public UniTask PlaySetCardVisibility(CancellationToken token, int handIndex, bool isVisible)
        {
            var drawCardView = _handAreaView.GetCardView(handIndex);
            drawCardView.IsVisible = isVisible;
            return UniTask.CompletedTask;
        }
        private UniTask PlaySetCardVisibility(CancellationToken token, in BattleAnim anim) => PlaySetCardVisibility(token, anim.animArgI0, anim.animArgI1 != 0);
        #endregion
        
        #region SacrificeCardFromHandAnim
        public void QueueSacrificeCardFromHandAnim(int handIndex) => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.SacrificeCardFromHand,
            animArgI0 = handIndex
        });
        public async UniTask PlaySacrificeCardFromHandAnim(CancellationToken token, int handIndex)
        {
            var drawCardView = _handAreaView.GetCardView(handIndex);
            drawCardView.IsVisible = true;
            await drawCardView.PlaySacrificeCardAnim(token);
        }
        private UniTask PlaySacrificeCardFromHandAnim(CancellationToken token, in BattleAnim anim) => PlaySacrificeCardFromHandAnim(token, anim.animArgI0);
        #endregion
        
        #region PlayerAttackEnemyAnim
        public void QueuePlayerAttackEnemyAnim() => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.PlayerAttackEnemy
        });
        public async UniTask PlayPlayerAttackEnemyAnim(CancellationToken token)
        {
            // todo 播放玩家攻击动画
            await _handAreaView.PlayPlayerAttackEnemyAnim(token);
            PlayShakeCameraAnim(token, 0.25f).Forget();
        }
        private UniTask PlayPlayerAttackEnemyAnim(CancellationToken token, in BattleAnim anim) => PlayPlayerAttackEnemyAnim(token);
        #endregion
        
        #region EnemyAttackPlayerAnim
        public void QueueEnemyAttackPlayerAnim() => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.EnemyAttackPlayer
        });
        public async UniTask PlayEnemyAttackPlayerAnim(CancellationToken token)
        {
            // todo 播放敌人攻击动画
            var enemyAnim = _enemyView.PlayAttackAnim(token);
            //给敌人攻击动画预留的等待时间
            await UniTask.WaitForSeconds(0.4f, cancellationToken: token);
            PlayShakeCameraAnim(token, 0.25f).Forget();
            await UniTask.WaitForSeconds(0.05f, cancellationToken: token);
            _handAreaView.PlayEnemyAttackPlayerAnim(token).Forget();
            await enemyAnim;
        }
        private UniTask PlayEnemyAttackPlayerAnim(CancellationToken token, in BattleAnim anim) => PlayEnemyAttackPlayerAnim(token);
        #endregion

        #region EnemyTakeDamageAnim
        public void QueueEnemyTakeDamageAnim() => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.EnemyTakeDamage
        });

        public async UniTask PlayeEnemyTakeDamageAnim(CancellationToken token)
        {
            var enemyAnim = _enemyView.PlayTakeDamageAnim(token);
            await enemyAnim;
        }
        private UniTask PlayEnemyTakeDamageAnim(CancellationToken token, in BattleAnim anim) => PlayeEnemyTakeDamageAnim(token);
        #endregion

        #region EnemyTakeBuffDamageAnim

        public void QueueEnemyTakeBuffDamageAnim() => QueueAnim(new BattleAnim
        {
            animType = BattleAnimType.EnemyTakeBuffDamage
        });

        public async UniTask PlayeEnemyTakeBuffDamageAnim(CancellationToken token)
        {
            var enemyAnim = _enemyView.PlayTakeBuffDamageAnim(token);
            await enemyAnim;
        }
        private UniTask PlayEnemyTakeBuffDamageAnim(CancellationToken token, in BattleAnim anim) => PlayeEnemyTakeBuffDamageAnim(token);

        #endregion
    }
}