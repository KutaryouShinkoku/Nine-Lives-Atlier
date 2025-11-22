using System;
using Game.Data;
using Game.Model;
using UnityEditor;
using WanFramework.UI.DataComponent;

namespace Game.Logic
{
    public static class LevelLogic
    {
        public static bool HasState(LevelTable.Entry level, LevelInnerState state)
        {
            return state switch
            {
                LevelInnerState.Battle => level.HasNormalBattle,
                LevelInnerState.Shop => level.HasShop,
                LevelInnerState.Boss => level.HasBoss,
                LevelInnerState.NextLevel => true,
                _ => true
            };
        }
        public static bool HasState(LevelInnerState state)
            => HasState(DataModel<InGameModel>.Instance.CurrentLevel.Data(), state);
        public static bool HasNextLevel()
        {
            var m = DataModel<InGameModel>.Instance;
            var level = m.CurrentLevel.Data();
            return m.LevelInnerState switch
            {
                LevelInnerState.Battle => level.HasShop || level.HasBoss,
                LevelInnerState.Shop => level.HasBoss,
                LevelInnerState.Boss => level.NextLevelPool.Data().Levels.Length != 0,
                _ => false
            };
        }
        /// <summary>
        /// 尝试进入下一关
        /// </summary>
        /// <returns>返回false则代表后续无关卡，即通关</returns>
        public static bool TryGotoNextLevel()
        {
            var m = DataModel<InGameModel>.Instance;
            // 先尝试增加关卡内部状态
            var levelState = m.LevelInnerState;
            ++levelState;
            while (!HasState(levelState)) ++levelState;
            if (levelState < LevelInnerState.NextLevel)
            {
                m.LevelInnerState = levelState;
                return true;
            }
            // 当前关卡通关，前往下一个非空大关卡
            ++m.PassedLevelCount;
            var level = m.CurrentLevel.Data();
            LevelIds levelId;
            do
            {
                var nextLevelPool = level.NextLevelPool.Data();
                if (nextLevelPool.Levels.Length == 0) // 没关卡了
                    return false;
                levelId = Algorithm.Roulette(nextLevelPool.Levels, nextLevelPool.Weights);
                level = levelId.Data();
                m.AttackScale *= 1.0f + level.AtkAdditionScale;
                m.HealthScale *= 1.0f + level.HealthAdditionScale;
            } while (!level.HasNormalBattle && !level.HasShop && !level.HasBoss);
            m.CurrentLevel = levelId;
            // 确保内部状态正确
            levelState = LevelInnerState.Battle;
            while (!HasState(levelState)) ++levelState;
            m.LevelInnerState = levelState;
            return true;
        }

        public static void SetLevel(LevelIds levelId)
        {
            var m = DataModel<InGameModel>.Instance;
            // 进入地图入口点，从池中选择地图
            m.CurrentLevel = levelId;
            var level = levelId.Data();
            m.AttackScale += level.AtkAdditionScale;
            m.HealthScale += level.HealthAdditionScale;
            // ++m.PassedLevelCount;
            // 空关卡直接调用GotoNext逻辑
            if (!level.HasNormalBattle && !level.HasShop && !level.HasBoss)
            {
                // 当前关卡为空，跳入下一关时减去当前关卡，空关卡不计入通关关卡数量
                --m.PassedLevelCount;
                TryGotoNextLevel();
            }
            else
            {
                // 寻找存在的关卡内部状态
                var levelState = LevelInnerState.Battle;
                while (!HasState(levelState)) ++levelState;
                m.LevelInnerState = levelState;
            }
        }
    }
}