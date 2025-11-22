using Game.Data;

namespace Game.Model.InGameSubModel
{
    public interface IBattleLogEntry
    {
        public string ToLocalString();
    }

    public class LogUseCard : IBattleLogEntry
    {
        public CardIds CardId { get; }
        public LogUseCard(CardIds cardId) => CardId = cardId;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogPlayerBeAttack : IBattleLogEntry
    {
        public float Damage { get; }
        public LogPlayerBeAttack(float damage) => Damage = damage;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogEnemyBeAttack : IBattleLogEntry
    {
        public float Damage { get; }
        public LogEnemyBeAttack(float damage) => Damage = damage;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogPlayerGainBuff : IBattleLogEntry
    {
        public BuffIds BuffId { get; }
        public int BuffStack { get; }
        public LogPlayerGainBuff(BuffIds buffId, int buffStack)
        {
            BuffId = buffId;
            BuffStack = buffStack;
        }
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogEnemyGainBuff : IBattleLogEntry
    {
        public BuffIds BuffId { get; }
        public int BuffStack { get; }
        public LogEnemyGainBuff(BuffIds buffId, int buffStack)
        {
            BuffId = buffId;
            BuffStack = buffStack;
        }
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogEnemyValueAChanged : IBattleLogEntry
    {
        public int Delta { get; }
        public LogEnemyValueAChanged(int delta) => Delta = delta;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogEnemyValueBChanged : IBattleLogEntry
    {
        public int Delta { get; }
        public LogEnemyValueBChanged(int delta) => Delta = delta;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogEnemyValueCChanged : IBattleLogEntry
    {
        public int Delta { get; }
        public LogEnemyValueCChanged(int delta) => Delta = delta;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogDrawCard : IBattleLogEntry
    {
        public int DrawCount { get; }
        public LogDrawCard(int drawCount) => DrawCount = drawCount;
        public string ToLocalString()
        {
            return "";
        }
    }
    public class LogDiscardCard : IBattleLogEntry
    {
        public int DiscardCount { get; }
        public LogDiscardCard(int discardCount) => DiscardCount = discardCount;
        public string ToLocalString()
        {
            return "";
        }
    }
}