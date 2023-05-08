public enum PawnClass
{
    Knight = 1,
    Archer = 2,
    Chemist = 3,
    Cleric = 4,
    Totem = 5,
    SkeletonWarrior = 6,
    SkeletonArcher = 7,
    SkeletonBomber = 8,
    SkeletonMedic = 9,
    SkeletonHero = 10
}

public enum PlayerTypeClass
{
    Totem,
    Character
}

public enum PawnStrategy
{
    Brute,
    ObjectiveSniper,
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
    SelectTileToAttack = 6,
    AttackTile = 7
}

public enum EnemyPhase
{
    FirstPhase,
    SecondPhase,
    NotAnEnemyPhase
}

public enum ForceCalculation
{
    ForceFree,
    ForceBeingCalculated,
    ForceBeingApplied
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

public enum WorldSide
{
    North,
    West,
    South,
    East
}