using Game.Data;
using Game.Model.InGameSubModel;
using WanFramework.UI.DataComponent;

namespace Game.Model
{
    public enum LevelInnerState
    {
        Battle = 0,
        Boss = 1,
        Shop = 2,
        NextLevel = 3,
    }
    /// <summary>
    /// 游戏中的数据模型，仅用于存储数据
    /// </summary>
    [ShowInDataModelEditor]
    public partial class InGameModel : DataModelBase
    {
        /// <summary>
        /// 牌库
        /// </summary>
        [AutoNotify]
        public DataCollection<CardModel> CardDeck { get; } = new();
        
        /// <summary>
        /// 角色Id
        /// </summary>
        [AutoNotify]
        private CharacterIds _characterId;
        
        /// <summary>
        /// 当前关卡
        /// </summary>
        [AutoNotify]
        private LevelIds _currentLevel;

        /// <summary>
        /// 当前通过的关卡数
        /// </summary>
        [AutoNotify]
        private int _passedLevelCount = 0;
        
        /// <summary>
        /// 关卡内部阶段
        /// </summary>
        [AutoNotify]
        private LevelInnerState _levelInnerState;
        
        [AutoNotify]
        private int _gold;

        [AutoNotify]
        private float _healthScale = 1.0f;
        
        [AutoNotify]
        private float _attackScale = 1.0f;

        /// <summary>
        /// 下一个关卡的Id
        /// </summary>
        [AutoNotify]
        private LevelIds _nextLevel;

        /// <summary>
        /// 下一个Boss敌人Id
        /// </summary>
        [AutoNotify]
        private EnemyIds _nextBoss;

        public override void Reset()
        {
            CardDeck.Clear();
            CurrentLevel = LevelIds.Unknown;
            LevelInnerState = LevelInnerState.Battle;
            CharacterId = CharacterIds.Unknown;
            Gold = 0;
            HealthScale = 1.0f;
            AttackScale = 1.0f;
            PassedLevelCount = 0;
            _nextLevel = LevelIds.Unknown;
            _nextBoss = EnemyIds.Unknown;
        }
    }
}
