using System;
using System.Collections.Generic;

namespace Game.Data
{
    /// <summary>
    /// 卡牌费用，支持多色
    /// </summary>
    [Serializable]
    public struct CardCost : IEquatable<CardCost>
    {
        public int Fire { get; set; }
        public int Water { get; set; }
        public int Earth { get; set; }
        public int Air { get; set; }

        /// <summary>
        /// 获取总费用
        /// </summary>
        public int TotalCost => Fire + Water + Earth + Air;

        /// <summary>
        /// 设置指定类型的费用
        /// </summary>
        /// <param name="type">费用类型</param>
        /// <param name="value">费用数值</param>
        public void SetCost(CardBaseType type, int value)
        {
            switch (type)
            {
                case CardBaseType.Fire:
                    Fire = value;
                    break;
                case CardBaseType.Water:
                    Water = value;
                    break;
                case CardBaseType.Earth:
                    Earth = value;
                    break;
                case CardBaseType.Air:
                    Air = value;
                    break;
                default:
                    throw new ArgumentException("Invalid CardBaseType");
            }
        }

        /// <summary>
        /// 获取所有费用
        /// </summary>
        /// <returns>费用字典</returns>
        public Dictionary<CardBaseType, int> GetAllCosts()
        {
            return new Dictionary<CardBaseType, int>
            {
                { CardBaseType.Fire, Fire },
                { CardBaseType.Water, Water },
                { CardBaseType.Earth, Earth },
                { CardBaseType.Air, Air }
            };
        }

        public void Reset()
        {
            Fire = 0;
            Water = 0;
            Earth = 0;
            Air = 0;
        }

        public bool Equals(CardCost other)
        {
            return Fire == other.Fire && Water == other.Water && Earth == other.Earth && Air == other.Air;
        }

        public override bool Equals(object obj)
        {
            return obj is CardCost other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Fire, Water, Earth, Air);
        }

        public static bool operator ==(CardCost lhs, CardCost rhs) => lhs.Equals(rhs);
        public static bool operator !=(CardCost lhs, CardCost rhs) => !lhs.Equals(rhs);

        #region 重载加法与减法运算符

        public static CardCost operator +(CardCost lhs, CardCost rhs)
        {
            return new CardCost
            {
                Fire = Math.Min(lhs.Fire + rhs.Fire, 10),
                Water = Math.Min(lhs.Water + rhs.Water, 10),
                Earth = Math.Min(lhs.Earth + rhs.Earth, 10),
                Air = Math.Min(lhs.Air + rhs.Air, 10)
            };
        }

        public static CardCost operator -(CardCost lhs, CardCost rhs)
        {
            return new CardCost
            {
                Fire = lhs.Fire - rhs.Fire,
                Water = lhs.Water - rhs.Water,
                Earth = lhs.Earth - rhs.Earth,
                Air = lhs.Air - rhs.Air
            };
        }

        #endregion
    }
}
