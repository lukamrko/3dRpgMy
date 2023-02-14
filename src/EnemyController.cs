using System.Linq;
using Godot;
using System;

public class EnemyController : Spatial
{
    private EnemyPhase _currentPhase = EnemyPhase.FirstPhase;
    EnemyStage stage = EnemyStage.ChoosePawn;
    EnemyStage Stage 
    {
        get{return stage;} 
        set{stage=value;}
    }
    EnemyPawn CurrentPawn;
    PlayerPawn AttackablePawn;

    TacticsCamera TacticsCamera = null;
    Arena Arena = null;
    PlayerController Targets = null;
    Godot.Collections.Array<PlayerPawn> TargetPawns;
    Godot.Collections.Array<EnemyPawn> EnemyPawns;

    public EnemyController()
    {
        // _Ready();
    }
    public bool CanFirstAct()
    {
        foreach (EnemyPawn p in EnemyPawns)
            if (p.EnemyCanFirstAct())
                return true;
        //TODO improve this condition
        // return Stage != EnemyStage.MovePawn;

        return false;
    }

    public bool CanSecondAct()
    {
        foreach (EnemyPawn p in EnemyPawns)
            if (p.EnemyCanSecondAct())
                return true;
        // return Stage != EnemyStage.MovePawn;
        return false;
    }

    public void Reset()
    {
        foreach (EnemyPawn p in EnemyPawns)
            p.Reset();
    }

    public void Configure(Arena arena, TacticsCamera camera)
    {
        TacticsCamera = camera;
        Arena = arena;
        Godot.Collections.Array pawns = GetChildren();
        if (pawns.Count > 0)
            CurrentPawn = pawns[0] as EnemyPawn;
    }

    public void ChoosePawn()
    {
        Arena.Reset();
        foreach (EnemyPawn pawn in EnemyPawns)
        {
            if (pawn.EnemyCanFirstAct())
                CurrentPawn = pawn;
        }
        Stage = EnemyStage.ChoseNearestEnemy;
    }

    public void ChoseNearestEnemy()
    {
        Arena.Reset();
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.JumpHeight, EnemyPawns);
        Arena.MarkReachableTiles(CurrentPawn.GetTile(), CurrentPawn.MoveRadius);
        Tile to = Arena.GetNearestNeighborToPawn(CurrentPawn, Targets.GetChildren().As<PlayerPawn>());
        CurrentPawn.PathStack = Arena.GeneratePathStack(to);
        TacticsCamera.Target = to;
        Stage = EnemyStage.MovePawn;
    }

    public void MovePawn()
    {
        if (CurrentPawn.PathStack.Count == 0)
        {     
            Stage = EnemyStage.ChosePawnToAttack;
            CurrentPawn.CanMove = false;
            ChoosePawnToAttack();
        }
    }

    public void ChoosePawnToAttack()
    {
        Arena.Reset();
        Godot.Collections.Array<EnemyPawn> emptyArray = null;
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius, emptyArray);
        Arena.MarkAttackableTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius);
        // AttackablePawn = Arena.GetWeakestPawnToAttack(TargetPawns);
        AttackablePawn = Arena.GetRandomPawnToAttack(TargetPawns);
        CurrentPawn.AttackingTowards = GetNormalizedVectorWhereToAttack();
        if (AttackablePawn != null)
        {
            AttackablePawn.DisplayPawnStats(true);
            TacticsCamera.Target = AttackablePawn;
        }
        Stage = EnemyStage.AttackPawn;
    }

    private Vector3? GetNormalizedVectorWhereToAttack()
    {
        if(AttackablePawn is null)
        {
            GD.Print("I, the great rattle bones skeleton {0} am unable to attack", CurrentPawn.PawnName);
            return null;
        }
        Vector3 attackingTowardsNormalized = this.Translation.Normalized()-AttackablePawn.Translation.Normalized();
        Vector3 attackingTowards = this.Translation - AttackablePawn.Translation;
        GD.Print(String.Format("I, the great rattle bones skeleton {0} am attacking towards this normalized position: {1} otherwise known {2}. Name of nemesis is {3}", 
            CurrentPawn.PawnName, attackingTowardsNormalized.ToString(), attackingTowards.ToString(), AttackablePawn.PawnName)
            );
        return attackingTowardsNormalized;
    }

    public void AttackPawn(float delta)
    {
        if (AttackablePawn == null)
            CurrentPawn.CanAttack = false;
        else
        {
            if (!CurrentPawn.DoAttack(AttackablePawn, delta))
                return;
            AttackablePawn.DisplayPawnStats(true);
            TacticsCamera.Target = CurrentPawn;
        }
        AttackablePawn = null;
        Stage = EnemyStage.ChoosePawn;
    }


    public override void _Ready()
    {
        Targets = GetParent().GetNode<PlayerController>("Player");
        TargetPawns = Targets.GetChildren().As<PlayerPawn>();
        EnemyPawns = GetChildren().As<EnemyPawn>();

    }

    public void FirstAct(float delta)
    {
        switch (Stage)
        {
            case EnemyStage.ChoosePawn:
                ChoosePawn();
                break;
            case EnemyStage.ChoseNearestEnemy:
                ChoseNearestEnemy();
                break;
            case EnemyStage.MovePawn:
                MovePawn();
                break;
            // case EnemyStage.ChosePawnToAttack:
            //     ChoosePawnToAttack();
            //     break;
            default:
                ChoosePawn();
                break;
        }
    }

    public void SecondAct(float delta)
    {
        switch (Stage)
        {
            case EnemyStage.AttackPawn:
                AttackPawn(delta);
                break;
            default:
                ChoosePawnThenPrepareAttack();
                break;
        }
    }

    private void ChoosePawnThenPrepareAttack()
    {
        foreach (EnemyPawn pawn in EnemyPawns)
        {
            if (pawn.EnemyCanSecondAct())
                CurrentPawn = pawn;
        }
        Stage = EnemyStage.AttackPawn;
    }
}


public enum EnemyStage
{
    ChoosePawn = 0,
    ChoseNearestEnemy = 1,
    MovePawn = 2,
    ChosePawnToAttack = 3,
    AttackPawn = 4
}