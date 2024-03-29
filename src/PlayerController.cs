using System.Linq;
using System.Diagnostics;
using Godot;
using System;
using Godot.Collections;

public partial class PlayerController : Node3D, IObserver
{
    private PlayerPawn _currentPawn = null;
    private PlayerSpawner PlayerSpawner;

    PlayerPawn CurrentPawn
    {
        get { return _currentPawn; }
        set
        {
            if (value is null && _currentPawn is object)
            {
                ResetSkipFieldOnFriendlyPawns();
            }
            _currentPawn = value;
        }
    }

    private void ResetSkipFieldOnFriendlyPawns()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
        {
            pawn.Skipped = false;
        }
    }

    float WaitTime = 0;

    bool IsJoyStick = false;

    Arena Arena = null;
    TacticsCamera TacticsCamera = null;

    PlayerStage Stage = PlayerStage.SelectPawn;

    PlayerControllerUI UIControl;
    public Godot.Collections.Array<PlayerPawn> PlayerPawns;
    Godot.Collections.Array<APawn> AllActiveUnits;
    private Tile AttackableTile;

    public void Configure(Arena myArena, TacticsCamera myCamera, PlayerControllerUI myControl)
    {
        Arena = myArena;
        TacticsCamera = myCamera;
        UIControl = myControl;
        TacticsCamera.Target = PlayerPawns[0];
        Button MoveButton = UIControl.GetAct("Move");
        Button WaitButton = UIControl.GetAct("Wait");
        Button CancelButton = UIControl.GetAct("Cancel");
        Button AttackButton = UIControl.GetAct("Attack");

        MoveButton.Pressed += PlayerWantsToMove;
        WaitButton.Pressed += PlayerWantsToWait;
        CancelButton.Pressed += PlayerWantsToCancel;
        AttackButton.Pressed += PlayerWantsToAttack;
    }

    public Godot.Collections.Array<PlayerPawn> InitialSpawn()
    {
        var actualPawns = new Godot.Collections.Array<PlayerPawn>();
        var pawns = PlayerSpawner.InitialPlayerSpawn();
        foreach (var pawn in pawns)
        {
            AddChild(pawn.Key);
            pawn.Key.GlobalPosition = pawn.Value;
            pawn.Key.Attach(this);
            actualPawns.Add(pawn.Key);
        }
        PlayerPawns.AddRangeAs(actualPawns);
        AllActiveUnits.AddRangeAs(actualPawns);
        return actualPawns;
    }

    public object GetMouseOverObject(uint lmask)
    {
        if (UIControl.IsMouseHoverButton())
        {
            return null;
        }
        Camera3D camera = GetViewport().GetCamera3D();
        Vector2 origin = GetViewport().GetMousePosition();

        Vector3 from = camera.ProjectRayOrigin(origin);
        Vector3 to = from + camera.ProjectRayNormal(origin) * 1000;
        var intersectingRay = new PhysicsRayQueryParameters3D
        {
            From = from,
            To = to,
            Exclude = null,
            CollisionMask = lmask
        };
        Godot.Collections.Dictionary rayIntersections = GetWorld3D().DirectSpaceState.IntersectRay(intersectingRay);
        if (rayIntersections.ContainsKey("collider"))
        {
            var result = rayIntersections["collider"].Obj;
            return result;
        }
        return null;
    }

    public bool CanAct()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
        {
            if (pawn.CanAct())
            {
                return true;
            }
        }

        return false;
    }

    public void Reset()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
        {
            if (!pawn.IsTotem)
            {
                pawn.Reset();
            }
        }
    }

    public void PlayerWantsToMove()
    {
        Stage = PlayerStage.DisplayAvailableMovements;
    }

    public void PlayerWantsToCancel()
    {
        Stage = (int)Stage > 1
            ? PlayerStage.DisplayAvailableActionsForPawn
            : PlayerStage.SelectPawn;
    }

    public void PlayerWantsToWait()
    {
        CurrentPawn.DoWait();
        Stage = PlayerStage.SelectPawn;
    }

    public void PlayerWantsToAttack()
    {
        Stage = PlayerStage.DisplayAttackableTargets;
    }

    private PlayerPawn AuxSelectPawn()
    {
        PlayerPawn pawn = GetMouseOverObject(2) as PlayerPawn;
        Tile tile = pawn is null
            ? GetMouseOverObject(1) as Tile
            : pawn.GetTile();
        Arena.MarkHoverTile(tile);
        
        if(tile is object &&
            tile.GetObjectAbove() is PlayerPawn playerPawn)
        {
            return playerPawn;
        }
        if (pawn is object)
        {
            return pawn;
        }
        else
        {
            return null;
        }
    }

    private Tile AuxSelectTile()
    {
        PlayerPawn pawn = GetMouseOverObject(2) as PlayerPawn;
        Tile tile = pawn is null
            ? GetMouseOverObject(1) as Tile
            : pawn.GetTile();
        Arena.MarkHoverTile(tile);
        return tile;
    }

    #region Stages
    public void SelectPawn()
    {
        CurrentPawn = AuxSelectPawn();
        if (CurrentPawn is null)
        {
            return;
        }

        if (Input.IsActionJustPressed("ui_accept")
            && CurrentPawn.CanAct()
            && PlayerPawns.Contains(CurrentPawn))
        {
            TacticsCamera.Target = CurrentPawn;
            Stage = PlayerStage.DisplayAvailableActionsForPawn;
        }
    }

    public void DisplayAvailableActionsForPawn()
    {
        Arena.Reset();
        var currentPawnTile = CurrentPawn.GetTile();
        Arena.MarkHoverTile(currentPawnTile);
    }

    public void DisplayAvailableMovements()
    {
        Arena.Reset();
        TacticsCamera.Target = CurrentPawn;
        var currentTile = CurrentPawn.GetTile();
        Arena.LinkTiles(currentTile, CurrentPawn.JumpHeight, PlayerPawns);
        Arena.MarkReachableTiles(currentTile, CurrentPawn.MoveRadius);
        Stage = PlayerStage.SelectNewLocation;
    }

    public void DisplayAttackableTargets()
    {
        Arena.Reset();
        TacticsCamera.Target = CurrentPawn;
        Godot.Collections.Array<PlayerPawn> emptyArray = null;
        var currentTile = CurrentPawn.GetTile();
        Arena.LinkTilesForAttack(currentTile, CurrentPawn.AttackRadius, emptyArray);
        Arena.MarkAttackableTiles(currentTile, CurrentPawn.AttackRadius);
        Stage = PlayerStage.SelectTileToAttack;
    }

    public void SelectNewLocation()
    {
        Tile tile = GetMouseOverObject(1) as Tile;
        Arena.MarkHoverTile(tile);
        if (Input.IsActionJustPressed("ui_accept")
            && tile is object
            && tile.Reachable)
        {
            CurrentPawn.PathStack = Arena.GeneratePathStack(tile);
            TacticsCamera.Target = tile;
            Stage = PlayerStage.MovePawn;
        }
    }

    public void SelectTileToAttack()
    {
        Tile tile = AuxSelectTile();
        if (tile is object)
        {
            AttackableTile = tile;
        }
        else
        {
            AttackableTile = null;
        }

        if (Input.IsActionJustPressed("ui_accept")
            && tile is object
            && tile.Attackable)
        {
            TacticsCamera.Target = AttackableTile;
            Stage = PlayerStage.AttackTile;
            Arena.Reset();
        }
    }

    public void MovePawn()
    {
        if (CurrentPawn.PathStack.Count == 0)
        {
            CurrentPawn.CanMove = false;
            SetCurrentPawnState();
        }
    }

    public void AttackTile()
    {
        CurrentPawn.DoCharacterActionOnTile(AllActiveUnits, AttackableTile);
        TacticsCamera.Target = CurrentPawn;

        SetCurrentPawnState();
    }

    private void SetCurrentPawnState()
    {
        if (!CurrentPawn.CanAct())
        {
            Stage = PlayerStage.SelectPawn;
        }
        else
        {
            Stage = PlayerStage.DisplayAvailableActionsForPawn;
        }
    }
    #endregion

    public void Act(double delta)
    {
        ListenShortcuts();
        if (CurrentPawn is null)
        {
            Stage = PlayerStage.SelectPawn;
        }
        var visibilityBasedOnStage = VisibilityBasedOnStage();
        UIControl.SetVisibilityOfActionsMenu(visibilityBasedOnStage, CurrentPawn);
        switch (Stage)
        {
            case PlayerStage.SelectPawn:
                SelectPawn();
                break;
            case PlayerStage.DisplayAvailableActionsForPawn:
                DisplayAvailableActionsForPawn();
                break;
            case PlayerStage.DisplayAvailableMovements:
                DisplayAvailableMovements();
                break;
            case PlayerStage.SelectNewLocation:
                SelectNewLocation();
                break;
            case PlayerStage.MovePawn:
                MovePawn();
                break;
            case PlayerStage.DisplayAttackableTargets:
                DisplayAttackableTargets();
                break;
            case PlayerStage.SelectTileToAttack:
                SelectTileToAttack();
                break;
            case PlayerStage.AttackTile:
                AttackTile();
                break;
        }
    }

    private void ListenShortcuts()
    {
        PlayerShortcuts();
    }

    private void PlayerShortcuts()
    {
        if (Input.IsActionJustPressed("ui_get_next_pawn"))
        {
            FastGetCurrentPawn();
            if (CurrentPawn is null)
            {
                FastGetCurrentPawn();
            }
        }
    }

    private void FastGetCurrentPawn()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
        {
            var currentPawnAllows = CurrentPawn is null || pawn.Skipped == false;
            var isCurrentPawn = pawn.CanAct() && currentPawnAllows;
            if (isCurrentPawn)
            {
                CurrentPawn = pawn;
                CurrentPawn.Skipped = true;
                Stage = PlayerStage.DisplayAvailableActionsForPawn;
                return;
            }
        }
        CurrentPawn = null;
    }

    public override void _Ready()
    {
        PlayerPawns = GetChildren().As<PlayerPawn>();
        EnemyController enemyController = GetParent().GetNode<EnemyController>("Enemy");
        var EnemyPawns = enemyController.GetChildren().As<EnemyPawn>();
        AllActiveUnits = new Godot.Collections.Array<APawn>();
        // AllActiveUnits.AddRangeAs(PlayerPawns);
        // AllActiveUnits.AddRangeAs(EnemyPawns);
        // AttachObserverToPawns(AllActiveUnits);
        PlayerSpawner = GetParent().GetNode<PlayerSpawner>("PlayerSpawner");
        GD.Print("Player spawner empty?");
    }

    private void AttachObserverToPawns(Array<APawn> pawns)
    {
        foreach (APawn pawn in pawns)
        {
            pawn.Attach(this);
        }
    }

    private PlayerStage[] ButtonVisibilityOnStages = new PlayerStage[]
    {
        PlayerStage.DisplayAvailableActionsForPawn,
        PlayerStage.DisplayAvailableMovements,
        PlayerStage.SelectNewLocation,
        PlayerStage.DisplayAttackableTargets,
        PlayerStage.SelectTileToAttack
    };

    private bool VisibilityBasedOnStage()
    {
        for (int i = 0; i < ButtonVisibilityOnStages.Length; i++)
        {
            if (Stage == ButtonVisibilityOnStages[i])
            {
                return true;
            }
        }

        return false;
    }

    public override void _Input(InputEvent @event)
    {
        IsJoyStick = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;
        UIControl.IsJoyStick = IsJoyStick;
    }

    public void Update(ISubject subject)
    {
        var abstractPawn = subject as APawn;
        AllActiveUnits.Remove(abstractPawn);

        if (subject is PlayerPawn playerPawn)
        {
            PlayerPawns.Remove(playerPawn);

            if (CurrentPawn != null
                && CurrentPawn.Equals(playerPawn))
            {
                CurrentPawn = null;
            }
        }
    }

    public void NotifyAboutNewEnemies(Array<EnemyPawn> enemies)
    {
        foreach (var enemyPawn in enemies)
        {
            enemyPawn.Attach(this);
            AllActiveUnits.Add(enemyPawn);
        }
    }

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
        foreach (var pawn in PlayerPawns)
        {
            if (pawn.shouldBeForciblyMoved
                && pawn.PawnClass != PawnClass.Totem)
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
        if (CurrentPawn.PathStack.Count == 0)
        {
            _forceCalculation = ForceCalculation.ForceFree;
            CurrentPawn.shouldBeForciblyMoved = false;
        }
    }
    #endregion
}

