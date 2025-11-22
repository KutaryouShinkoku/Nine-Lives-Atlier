using System;
using Game.Data;
using WanFramework.UI.DataComponent;

namespace Game.Model.InGameSubModel
{
    /// <summary>
    /// 敌人意图
    /// </summary>
    public enum EnemyAttempt
    {
        /// <summary>
        /// 无意图
        /// </summary>
        None = 0,
        /// <summary>
        /// 攻击
        /// </summary>
        Attack = 1,
        /// <summary>
        /// 战技
        /// </summary>
        Skill = 2
    }

    /// <summary>
    /// 敌人模型
    /// </summary>
    public partial class EnemyModel : DataModelBase
    {
        [AutoNotify]
        private int _health;
        [AutoNotify]
        private int _attack;
        [AutoNotify]
        private int _reward;
        [AutoNotify]
        private EnemyAttempt _attempt;
        [AutoNotify]
        private EnemyIds _enemyId;

        public DataCollection<BuffModel> enemyBuffs { get; } = new();
        
        public override void Reset()
        {
            enemyBuffs.Clear();
            Health = 0;
            Attack = 0;
            Reward = 0;
        }

        public bool CheckBuffByID(BuffIds buffId)
        {
            foreach (var buff in enemyBuffs)
            {
                if (buff.Id == buffId)
                    return true;
            }
            return false;
        }
    }
}
