using System.Linq;
using Godot;
using System;

public class EnemyController : Spatial
{
    int Stage = 0;
    Pawn CurrentPawn;
    Pawn AttackablePawn;

    TacticsCamera TacticsCamera = null;
    Arena Arena = null;
    PlayerController Targets = null;
    Godot.Collections.Array<Pawn> TargetPawns;

    public EnemyController()
    {
        // _Ready();
    }
    public bool CanAct()
    {
        foreach (Pawn p in GetChildren())
            if (p.CanAct())
                return true;
        return Stage > 0;
    }

    public void Reset()
    {
        foreach (Pawn p in GetChildren())
            p.Reset();
    }

    public void Configure(Arena arena, TacticsCamera camera)
    {
        TacticsCamera = camera;
        Arena = arena;
        Godot.Collections.Array pawns = GetChildren();
        if (pawns.Count > 0)
            CurrentPawn = pawns[0] as Pawn;
    }

    public void ChoosePawn()
    {
        Arena.Reset();
        var pawns = GetChildren();
        foreach (var pawnObj in pawns)
        {
            Pawn pawn = pawnObj as Pawn;
            if (pawn.CanAct())
                CurrentPawn = pawn;
        }
        Stage = 1;
    }

    public void ChoseNearestEnemy()
    {
        Arena.Reset();
        Godot.Collections.Array<Pawn> allies = GetChildren().As<Pawn>();
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.JumpHeight, allies);
        Arena.MarkReachableTiles(CurrentPawn.GetTile(), CurrentPawn.MoveRadius);
        Tile to = Arena.GetNearestNeighborToPawn(CurrentPawn, Targets.GetChildren().As<Pawn>());
        CurrentPawn.PathStack = Arena.GeneratePathStack(to);
        TacticsCamera.Target = to;
        Stage = 2;
    }

    public void MovePawn()
    {
        if (CurrentPawn.PathStack.Count == 0)
            Stage = 3;
    }

    public void ChoosePawnToAttack()
    {
        Arena.Reset();
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius);
        Arena.MarkAttackableTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius);
        AttackablePawn = Arena.GetWeakestPawnToAttack(TargetPawns);
        if (AttackablePawn != null)
        {
            AttackablePawn.DisplayPawnStats(true);
            TacticsCamera.Target = AttackablePawn;
        }
        Stage = 4;
    }

    public void AttackPawn(float delta)
    {
        if (AttackablePawn == null)
            CurrentPawn.CanAttack = false;
        else
        {
            if (!CurrentPawn.DoAttack(AttackablePawn, delta))
                return;
            AttackablePawn.DisplayPawnStats(false);
            TacticsCamera.Target = CurrentPawn;
        }
        AttackablePawn = null;
        Stage = 0;
    }


    public override void _Ready()
    {
        Targets = GetParent().GetNode<PlayerController>("Player");
        TargetPawns = Targets.GetChildren().As<Pawn>();

    }

    public void Act(float delta)
    {
        switch (Stage)
        {
            case 0:
                ChoosePawn();
                break;
            case 1:
                ChoseNearestEnemy();
                break;
            case 2:
                MovePawn();
                break;
            case 3:
                ChoosePawnToAttack();
                break;
            case 4:
            default:
                AttackPawn(delta);
                break;
        }
    }



    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
