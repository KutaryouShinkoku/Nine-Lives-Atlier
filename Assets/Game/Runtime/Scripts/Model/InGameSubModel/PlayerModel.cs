using Game.Model.InGameSubModel;
using Game.Data;
using WanFramework.UI.DataComponent;

namespace Game.Model.InGameSubModel
{
    /// <summary>
    /// 玩家模型，仅用于存储数据
    /// </summary>
    public partial class PlayerModel : DataModelBase
    {
        [AutoNotify]
        private int _health;
        [AutoNotify]
        private int _maxHealth;
        [AutoNotify]
        private CardCost _resources;
        [AutoNotify]
        private CharacterIds _characterId;
        [AutoNotify]
        private bool _isFocusCondition = false;
        [AutoNotify]
        private int _focusArgs = 0;
        [AutoNotify]
        private int _tailwind = 0;

        public DataCollection<BuffModel> Buffs { get; } = new();
        
        public override void Reset()
        {
            _resources.Reset();
            Buffs.Clear();
            Health = 0;
            MaxHealth = 0;
            CharacterId = CharacterIds.Unknown;
        }

        public bool CheckBuffByID(BuffIds buffId)
        {
            foreach (var buff in Buffs)
            {
                if (buff.Id == buffId)
                    return true;
            }
            return false;
        }
    }
}
