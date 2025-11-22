using Game.Data;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.BattleAnim
{
    public enum BattleAnimType
    {
        DrawCardToHand,
        UseCardFromHand,
        DiscardCard,
        SacrificeCardFromHand,
        PlayerAttackEnemy,
        HealthChange,
        SetCardVisibility,
        
        //敌人动画
        EnemyAttackChange,
        EnemyRewardChange,
        EnemyHealthChange,
        EnemyAttackPlayer,
        EnemyTakeDamage,
        EnemyTakeBuffDamage,

        MoveCardToEnemy,
        MoveCardToResource,
        
        ShakeCamera,
        
        //弹数字和Buff
        DamagePopup,
        BuffPopup,
        BuffStackPopup,
        EnemyBuffStackPopup,

        //Buff动画
        BuffStackAdd,
        EnemyBuffStackAdd,
        BuffStackMinus,
        EnemyBuffStackMinus,
        BuffEffect,
        EnemyBuffEffect,
        BuffStackNumChange,
        EnemyBuffStackNumChange,

        //Buff显示动画
        BuffShow,
        EnemyBuffShow,

        //ResourceBar动画
        ResourceBarValChange,
    }

    [Serializable]
    public struct BattleAnim
    {
        public BattleAnimType animType;
        public int animArgI0;
        public int animArgI1;
        public int animArgI2;
        public float animArgF0;
        public float animArgF1;
        public float animArgF2;
        public Sprite animArgObj0;
        public CardCost animArgCardCost;
    }
}
