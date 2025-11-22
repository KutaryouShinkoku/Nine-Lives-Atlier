using Game.Data;
using Game.Model.InGameSubModel;
using System;
using WanFramework.UI.DataComponent;

namespace Game.Model
{
    /// <summary>
    /// 游戏中的数据模型，仅用于存储数据
    /// </summary>
    [ShowInDataModelEditor]
    public partial class BattleModel : DataModelBase
    {
        public const int MaxHandCards = 5;

        [AutoNotify]
        private int _additionalMaxHandCount = 0;

        /// <summary>
        /// 计算最终有效的手牌上限：基础(5) + 额外加成，最大不超过10
        /// </summary>
        public int EffectiveMaxHandCards => Math.Min(MaxHandCards + _additionalMaxHandCount, 10);

        /// <summary>
        /// 玩家模型
        /// </summary>
        [AutoNotify]
        public PlayerModel PlayerModel { get; } = new();

        /// <summary>
        /// 敌人模型，最多1个敌人
        /// </summary>
        [AutoNotify]
        public EnemyModel EnemyModel { get; } = new();

        /// <summary>
        /// 手牌数据模型
        /// </summary>
        [AutoNotify]
        public DataCollection<CardModel> HandCards { get; } = new();

        /// <summary>
        /// 弃牌区
        /// </summary>
        [AutoNotify]
        public DataCollection<CardModel> DiscardCards { get; } = new();

        /// <summary>
        /// 剩余未抽完卡牌
        /// </summary>
        [AutoNotify]
        public DataCollection<CardModel> RemainCards { get; } = new();

        [AutoNotify]
        private int _drawCardReduction;
        [AutoNotify]
        private int _drawCardIncrease;
        
        [AutoNotify]
        private int _turn;
        
        [AutoNotify]
        public DataCollection<IBattleLogEntry> BattleLogs { get; } = new();
        
        public override void Reset()
        {
            BattleLogs.Clear();
            RemainCards.Clear();
            DiscardCards.Clear();
            HandCards.Clear();
            EnemyModel.Reset();
            PlayerModel.Reset();
            AdditionalMaxHandCount = 0;
            DrawCardReduction = 0;
            DrawCardIncrease = 0;
            Turn = 0;
        }
    }
}
