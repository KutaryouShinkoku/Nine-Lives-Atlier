using System;

namespace Game.Data
{
    /// <summary>
    /// 卡牌类型，决定卡牌底色
    /// </summary>
    public enum CardBaseType
    {
        Fire,
        Water,
        Earth,
        Air,
        Fire_Water,
        Fire_Earth,
        Fire_Air,
        Water_Earth,
        Water_Air,
        Earth_Air,
    }

    public enum CardLabel
    {
        Fragile,
        Toxic,
        HighTemp,
        MagicLock,
    }

    /// <summary>
    /// 卡牌稀有度
    /// </summary>
    public enum CardRarity
    {
        Basic,
        Normal,
        Rare,
        Shop
    }

    public enum CardCondition
    {
        None,
        Surge,
        Madness,
        Unstable,
        Cooldown,
        Peace,
        Focus
    }

    /// <summary>
    /// 卡牌池类型，决定卡牌加入的奖励/商店池
    /// </summary>
    [Flags]
    public enum CardPool
    {
        Fire = 1 << 0,
        Water = 1 << 1,
        Earth = 1 << 2,
        Air = 1 << 3,

        All = Fire | Water | Earth | Air
    }
}
