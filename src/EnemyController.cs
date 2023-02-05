using System.Linq;
using Godot;
using System;

public class EnemyController : Spatial
{
    EnemyStage Stage = EnemyStage.ChoosePawn;
    EnemyPawn CurrentPawn;
    PlayerPawn AttackablePawn;

    TacticsCamera TacticsCamera = null;
    Arena Arena = null;
    PlayerController Targets = null;
    Godot.Collections.Array<PlayerPawn> TargetPawns;

    public EnemyController()
    {
        // _Ready();
    }
    public bool CanFirstAct()
    {
        foreach (EnemyPawn p in GetChildren().As<EnemyPawn>())
            if (p.EnemyCanFirstAct())
                return true;
        return Stage != EnemyStage.ChoosePawn;
    }

    public bool CanSecondAct()
    {
        foreach (EnemyPawn p in GetChildren().As<EnemyPawn>())
            if (p.EnemyCanSecondAct())
                return true;
        return Stage != EnemyStage.ChoosePawn;
    }

    public void Reset()
    {
        foreach (EnemyPawn p in GetChildren().As<EnemyPawn>())
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
        var pawns = GetChildren();
        foreach (var pawnObj in pawns)
        {
            EnemyPawn pawn = pawnObj as EnemyPawn;
            //TODO change this then check
            if (pawn.EnemyCanFirstAct())
                CurrentPawn = pawn;
        }
        Stage = EnemyStage.ChoseNearestEnemy;
    }

    public void ChoseNearestEnemy()
    {
        Arena.Reset();
        Godot.Collections.Array<PlayerPawn> allies = GetChildren().As<PlayerPawn>();
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.JumpHeight, allies);
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
        }
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
        Stage = EnemyStage.AttackPawn;
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
        Stage = EnemyStage.ChoosePawn;
    }


    public override void _Ready()
    {
        Targets = GetParent().GetNode<PlayerController>("Player");
        TargetPawns = Targets.GetChildren().As<PlayerPawn>();

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
        }
    }

    public void SecondAct(float delta)
    {
        switch (Stage)
        {
            case EnemyStage.ChosePawnToAttack:
                ChoosePawnToAttack();
                break;
            case EnemyStage.AttackPawn:
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


public enum EnemyStage
{
    ChoosePawn = 0,
    ChoseNearestEnemy = 1,
    MovePawn = 2,
    ChosePawnToAttack = 3,
    AttackPawn = 4
}