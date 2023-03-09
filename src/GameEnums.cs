public enum PawnClass
{
    Knight,
    Archer,
    Chemist,
    Cleric,
    Skeleton,
    SkeletonCPT,
    SkeletonMage
}

public enum PawnStrategy
{
    Tank,
    Flank,
    Support
}

public enum PlayerStage
{
    SelectPawn = 0,
    DisplayAvailableActionsForPawn = 1,
    DisplayAvailableMovements = 2,
    SelectNewLocation = 3,
    MovePawn = 4,
    DisplayAttackableTargets = 5,
    SelectPawnToAttack = 6,
    AttackPawn = 7
}

public enum EnemyPhase
{
    FirstPhase,
    SecondPhase,
    NotAnEnemyPhase
}

public enum EnemyStage
{
    ChoosePawn = 0,
    ChoseNearestEnemy = 1,
    MovePawn = 2,
    ChosePawnToAttack = 3,
    AttackPawn = 4,
    ForceBeingCalculated = 5,
    ForceBeingApplied = 6
}