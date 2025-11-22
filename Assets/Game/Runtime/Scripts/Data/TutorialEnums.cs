namespace Game.Data
{
    public enum TutorialCondition
    {
        None,
        AnyKey,
        WaitForDrawCard,
        WaitForUseCard,
        WaitForSacrificeCard,
        CheckSacrificeCardIndex,
        CheckSacrificeCardId,
        CheckUseCardIndex,
        CheckUseCardId,
    }
    
    public enum TutorialAction
    {
        None,
        DrawCard,
        SacrificeLastCard,
        UseLastCard,
        SacrificeHandCard,
        UseHandCard,
        PlayEnemyAttack,
        PlayPlayerAttack,
        IncreaseResource,
        SetResource,
        SetCharacter,
        SetHealth,
        SetEnemy,
        SetEnemyHealth,
        SetEnemyAttack,
        SetEnemyReward,
        EnableDrag,
        DisableDrag,
        EnableDraw,
        DisableDraw,
        EnableSelect,
        DisableSelect,
        HealthChange,
        BuffStackChange,
        EnemyHealthChange,
        EnemyBuffStackChange,
        EnemyAttackChange,
        EnemyRewardChange,
        
        Conditional,
        Goto,
    }
}