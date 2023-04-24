using System.Linq;
using Godot;
using System;
using System.Threading.Tasks;

public partial class EnemyController : Node3D, IObserver
{
    private EnemyPhase _currentPhase = EnemyPhase.FirstPhase;
    EnemyStage stage = EnemyStage.ChoosePawn;
    EnemyStage Stage
    {
        get { return stage; }
        set { stage = value; }
    }
    EnemyPawn CurrentPawn;
    PlayerPawn AttackablePawn;

    TacticsCamera TacticsCamera = null;
    Arena Arena = null;
    PlayerController Targets = null;
    Godot.Collections.Array<PlayerPawn> PlayerPawns;
    Godot.Collections.Array<EnemyPawn> EnemyPawns;

    public Godot.Collections.Array<APawn> AllActiveUnits;

    Spawner Spawner;

    public bool CanFirstAct()
    {
        for (int i = 0; i < EnemyPawns.Count; i++)
        {
            EnemyPawn pawn = EnemyPawns[i];
            if (pawn.EnemyCanFirstAct())
            {
                return true;
            }
        }

        return false;
    }

    public bool CanSecondAct()
    {
        foreach (EnemyPawn p in EnemyPawns)
        {
            if (p.EnemyCanSecondAct())
            {
                return true;
            }
        }
        // return Stage != EnemyStage.MovePawn;
        return false;
    }

    public void Reset()
    {
        foreach (EnemyPawn p in EnemyPawns)
        {
            p.Reset();
        }
    }

    public void Configure(Arena arena, TacticsCamera camera)
    {
        TacticsCamera = camera;
        Arena = arena;
        if (EnemyPawns.Count > 0)
        {
            CurrentPawn = EnemyPawns[0];
        }
    }

    public void ChoosePawn()
    {
        Arena.Reset();
        foreach (EnemyPawn pawn in EnemyPawns)
        {
            if (pawn.EnemyCanFirstAct())
            {
                CurrentPawn = pawn;
            }
        }

        Stage = EnemyStage.ChoseNearestEnemy;
    }

    public void ChoseNearestEnemy()
    {
        Arena.Reset();
        Tile currentPawnTile = CurrentPawn.GetTile();
        if (currentPawnTile is null)
        {
            GD.Print("we are null: ", CurrentPawn.PawnName);
            currentPawnTile = CurrentPawn.GetTile();
        }
        Arena.LinkTiles(currentPawnTile, CurrentPawn.JumpHeight, EnemyPawns);
        Arena.MarkReachableTiles(CurrentPawn.GetTile(), CurrentPawn.MoveRadius);
        Tile to = Arena.GetNearestNeighborToPawn(CurrentPawn, PlayerPawns);
        CurrentPawn.PathStack = Arena.GeneratePathStack(to);
        TacticsCamera.Target = to;
        Stage = EnemyStage.MovePawn;
    }

    public void MovePawn()
    {
        TacticsCamera.Target = CurrentPawn;
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
        AttackablePawn = Arena.GetRandomPawnToAttack(PlayerPawns);
        CurrentPawn.AttackingTowards = GetDirectionToWhichShouldAttack();
        if (AttackablePawn != null)
        {
            AttackablePawn.DisplayPawnStats(true);
            TacticsCamera.Target = AttackablePawn;
        }
        Stage = EnemyStage.AttackPawn;
    }

    private Vector3? GetDirectionToWhichShouldAttack()
    {
        if (AttackablePawn is null)
        {
            GD.Print(String.Format("I, the great rattle bones skeleton {0} am unable to attack", CurrentPawn.PawnName));
            return null;
        }
        Vector3 attackDirectionRounded = AttackablePawn.Position.Rounded() - CurrentPawn.Position.Rounded();
        GD.Print(String.Format("I, the great rattle bones skeleton {0} am attacking towards this  position: {1}. Name of nemesis is {2}",
           CurrentPawn.PawnName, attackDirectionRounded, AttackablePawn.PawnName));
        return attackDirectionRounded;
    }

    public void Attack(double delta)
    {
        if (CurrentPawn.AttackingTowards is null)
        {
            CurrentPawn.CanAttack = false;
            return;
        }
        Vector3 attackingTowards = CurrentPawn.AttackingTowards.Value + CurrentPawn.Position.Rounded();
        CurrentPawn.DoAttackOnLocation(AllActiveUnits, attackingTowards);
        Stage = EnemyStage.ChoosePawn;
    }


    public override void _Ready()
    {
        Targets = GetParent().GetNode<PlayerController>("Player");
        PlayerPawns = Targets.GetChildren().As<PlayerPawn>();
        EnemyPawns = GetChildren().As<EnemyPawn>();
        Spawner = GetParent().GetNode<Spawner>("EnemySpawner");
        AllActiveUnits = new Godot.Collections.Array<APawn>();
        AllActiveUnits.AddRangeAs(PlayerPawns);
        AllActiveUnits.AddRangeAs(EnemyPawns);
        AttachObserverToPawns(AllActiveUnits);
    }

    private void AttachObserverToPawns(Godot.Collections.Array<APawn> pawns)
    {
        foreach (APawn pawn in pawns)
        {
            pawn.Attach(this);
        }
    }

    public void FirstAct(double delta)
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

    public void SecondAct(double delta)
    {
        ChoosePawnThenPrepareAttack();
        if (Stage == EnemyStage.AttackPawn)
        {
            Attack(delta);
        }
    }

    private void ChoosePawnThenPrepareAttack()
    {
        foreach (EnemyPawn pawn in EnemyPawns)
        {
            if (pawn.EnemyCanSecondAct())
            {
                CurrentPawn = pawn;
                break;
            }
        }
        Stage = EnemyStage.AttackPawn;
    }

    // Inherited from IObserver
    public void Update(ISubject subject)
    {
        GD.Print("I got notification. Somebody probably died!");
        var abstractPawn = subject as APawn;
        AllActiveUnits.Remove(abstractPawn);

        if (subject is PlayerPawn)
        {
            var playerPawn = subject as PlayerPawn;
            PlayerPawns.Remove(playerPawn);
        }

        if (subject is EnemyPawn)
        {
            var enemyPawn = subject as EnemyPawn;
            EnemyPawns.Remove(enemyPawn);

            if (CurrentPawn != null
                && CurrentPawn.Equals(enemyPawn))
            {
                CurrentPawn = null;
            }
        }
    }

    public Godot.Collections.Array<EnemyPawn> SpawnEnemies()
    {
        Godot.Collections.Array<EnemyPawn> pawns = Spawner.SpawnEnemies();
        foreach (EnemyPawn pawn in pawns)
        {
            AddChild(pawn);
            pawn.Attach(this);
        }
        EnemyPawns.AddRangeAs(pawns);
        AllActiveUnits.AddRangeAs(pawns);
        return pawns;
    }

    private EnemyStage oldStage;

    #region OutsideForces
    private ForceCalculation _forceCalculation = ForceCalculation.ForceFree;
    public bool ShouldApplyForce()
    {
        if (_forceCalculation == ForceCalculation.ForceBeingApplied 
			&& CurrentPawn is object)
        {
            ApplyForce();
            return true;
        }
        foreach (var pawn in EnemyPawns)
        {
            if (pawn.shouldBeForciblyMoved
                && _forceCalculation != ForceCalculation.ForceBeingApplied)
            {
                _forceCalculation = ForceCalculation.ForceBeingCalculated;
                CurrentPawn = pawn;
                return true;
            }
        }
        return false;
    }

    public void DoForcedMovement(double delta)
    {
        if (_forceCalculation == ForceCalculation.ForceBeingCalculated)
        {
            CalculateForce();
            ApplyForce();
        }
        else
        {
            ApplyForce();
        }
    }

    private void CalculateForce()
    {
        var location = (CurrentPawn.GlobalPosition + CurrentPawn.directionOfForcedMovement).Rounded();
        Tile to = Arena.GetTileAtLocation(location);
        if (to is null)
        {
            CurrentPawn.PathStack.Add(location);
        }
        else
        {
            var currentPawnTile = CurrentPawn.GetTile();
            CurrentPawn.PathStack = Arena.GenerateSimplePathStack(currentPawnTile, to);
            TacticsCamera.Target = to;
        }
        CurrentPawn.MoveDirection = Vector3.Zero;
		_forceCalculation = ForceCalculation.ForceBeingApplied;
    }

    private void ApplyForce()
    {
        GD.Print("I entered apply force method");
        if (CurrentPawn.PathStack.Count == 0)
        {
            _forceCalculation = ForceCalculation.ForceFree;
            CurrentPawn.shouldBeForciblyMoved = false;
        }
    }
    #endregion

}


