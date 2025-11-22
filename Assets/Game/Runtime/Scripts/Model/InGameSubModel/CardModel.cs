using Game.Data;
using System;
using WanFramework.UI.DataComponent;

namespace Game.Model.InGameSubModel
{
    /// <summary>
    /// 卡牌模型，仅记录费用变化值和强化值
    /// </summary>
    public partial class CardModel : DataModelBase
    {
        [AutoNotify]
        private CardIds _id;

        [AutoNotify]
        private CardCost _feeChange;

        [AutoNotify]
        private int _additionEffectVal1;

        [AutoNotify]
        private int _additionEffectVal2;

        [AutoNotify]
        private int _additionEffectVal3;

        /// <summary>
        /// 重置卡牌的费用变化值和强化值
        /// </summary>
        public override void Reset()
        {
            _feeChange.Reset();
            _additionEffectVal1 = 0;
            _additionEffectVal2 = 0;
            _additionEffectVal3 = 0;
        }

        public CardModel Clone() => new()
        {
            _id = _id,
            _feeChange = _feeChange,
            _additionEffectVal1 = _additionEffectVal1,
            _additionEffectVal2 = _additionEffectVal2,
            _additionEffectVal3 = _additionEffectVal3
        };
    }
}
