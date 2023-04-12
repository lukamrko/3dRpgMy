using System.Diagnostics;
using Godot;
using System;
using Godot.Collections;

public partial class PlayerController : Node3D, IObserver
{
    private PlayerPawn _currentPawn = null;

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

    APawn AttackablePawn = null;

    float WaitTime = 0;

    bool IsJoyStick = false;

    Arena Arena = null;
    TacticsCamera TacticsCamera = null;

    PlayerStage Stage = PlayerStage.SelectPawn;

    PlayerControllerUI UIControl;
    Godot.Collections.Array<PlayerPawn> PlayerPawns;
    Godot.Collections.Array<APawn> AllActiveUnits;

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

        // MoveButton.Connect("pressed", new Callable(this, "PlayerWantsToMove"));
        // WaitButton.Connect("pressed", new Callable(this, "PlayerWantsToWait"));
        // CancelButton.Connect("pressed", new Callable(this, "PlayerWantsToCancel"));
        // AttackButton.Connect("pressed", new Callable(this, "PlayerWantsToAttack"));

        MoveButton.Pressed += PlayerWantsToMove;
        WaitButton.Pressed += PlayerWantsToWait;
        CancelButton.Pressed += PlayerWantsToCancel;
        AttackButton.Pressed += PlayerWantsToAttack;
    }

    public object GetMouseOverObject(uint lmask)
    {
        if (UIControl.IsMouseHoverButton())
        {
            return null;
        }
        Camera3D camera = GetViewport().GetCamera3D();
        // Vector2 origin = !IsJoyStick
        //     ? GetViewport().GetMousePosition()
        //     : GetViewport().Size / 2;
        Vector2 origin = GetViewport().GetMousePosition();

        Vector3 from = camera.ProjectRayOrigin(origin);
        Vector3 to = from + camera.ProjectRayNormal(origin) * 1000;
        var intersectingRay = new PhysicsRayQueryParameters3D
        {
            From=from,
            To = to,
            Exclude = null,
            CollisionMask = lmask
        };
        Godot.Collections.Dictionary rayIntersections = GetWorld3D().DirectSpaceState.IntersectRay(intersectingRay);
        // Godot.Collections.Dictionary rayIntersections = GetWorld3D().DirectSpaceState.IntersectRay(from, to, null, lmask);
        // if (rayIntersections.Contains("collider"))
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

        return (int)Stage > 0;
    }

    public void Reset()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
        {
            pawn.Reset();
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
        if (pawn is object)
        {
            return pawn;
        }
        else
        {
            if (tile is object)
            {
                return tile.GetObjectAbove() as PlayerPawn;
            }
            else
            {
                return null;
            }
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
        Arena.Reset();
        if (CurrentPawn is object)
        {
            CurrentPawn.DisplayPawnStats(false);
        }

        CurrentPawn = AuxSelectPawn();
        if (CurrentPawn is null)
        {
            return;
        }

        CurrentPawn.DisplayPawnStats(true);
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
        CurrentPawn.DisplayPawnStats(true);
        Arena.Reset();
        Arena.MarkHoverTile(CurrentPawn.GetTile());
    }

    public void DisplayAvailableMovements()
    {
        Arena.Reset();
        if (CurrentPawn is null)
        {
            return;
        }
        TacticsCamera.Target = CurrentPawn;
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.JumpHeight, PlayerPawns);
        Arena.MarkReachableTiles(CurrentPawn.GetTile(), CurrentPawn.MoveRadius);
        Stage = PlayerStage.SelectNewLocation;
    }

    public void DisplayAttackableTargets()
    {
        Arena.Reset();
        if (CurrentPawn is null)
        {
            return;
        }
        TacticsCamera.Target = CurrentPawn;
        Godot.Collections.Array<PlayerPawn> emptyArray = null;
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius, emptyArray);
        Arena.MarkAttackableTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius);
        Stage = PlayerStage.SelectPawnToAttack;
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

    public void SelectPawnToAttack()
    {
        CurrentPawn.DisplayPawnStats(true);
        if (AttackablePawn is object)
        {
            AttackablePawn.DisplayPawnStats(false);
        }

        Tile tile = AuxSelectTile();
        if (tile is object)
        {
            AttackablePawn = tile.GetObjectAbove() as APawn;
        }
        else
        {
            AttackablePawn = null;
        }

        if (AttackablePawn is object)
        {
            AttackablePawn.DisplayPawnStats(true);
        }

        if (Input.IsActionJustPressed("ui_accept") 
            && tile is object 
            && tile.Attackable)
        {
            TacticsCamera.Target = AttackablePawn;
            Stage = PlayerStage.AttackPawn;
        }
    }

    public void MovePawn()
    {
        CurrentPawn.DisplayPawnStats(false);
        if (CurrentPawn.PathStack.Count == 0)
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
            
    }

    public void AttackPawn(double delta)
    {
        if (AttackablePawn is null)
        {
            CurrentPawn.CanAttack = false;
        }
        else
        {
            CurrentPawn.DoAttack(AttackablePawn, AllActiveUnits);
            AttackablePawn.DisplayPawnStats(true);
            TacticsCamera.Target = CurrentPawn;
        }

        AttackablePawn = null;
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
            case PlayerStage.SelectPawnToAttack:
                SelectPawnToAttack();
                break;
            case PlayerStage.AttackPawn:
                AttackPawn(delta);
                break;
        }

    }

    private void ListenShortcuts()
    {
        MoveCamera();
        CameraRotation();
        PlayerShortcuts();
    }

    #region Camera
    public void MoveCamera()
    {
        if(Input.GetActionStrength("camera_left")!=0)
        {
            GD.Print("AAA");
        }
        float h = -Input.GetActionStrength("camera_left") + Input.GetActionStrength("camera_right");
        float v = Input.GetActionStrength("camera_forward") - Input.GetActionStrength("camera_backwards");
        TacticsCamera.MoveCamera(h, v, IsJoyStick);
    }

    public void CameraRotation()
    {
        if (Input.IsActionJustPressed("camera_rotate_left"))
        {
            TacticsCamera.YRot -= 90;
        }
        if (Input.IsActionJustPressed("camera_rotate_right"))
        {
            TacticsCamera.YRot += 90;
        }
    }
    #endregion 

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
        AllActiveUnits.AddRangeAs(PlayerPawns);
        AllActiveUnits.AddRangeAs(EnemyPawns);
        AttachObserverToPawns(AllActiveUnits);
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
            PlayerStage.SelectPawnToAttack
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

        if (subject is PlayerPawn)
        {
            var playerPawn = subject as PlayerPawn;
            PlayerPawns.Remove(playerPawn);
        }
    }

    internal void NotifyAboutNewEnemies(Array<EnemyPawn> enemies)
    {
        foreach(var enemyPawn in enemies)
        {
            enemyPawn.Attach(this);
            AllActiveUnits.Add(enemyPawn);
        }
    }
}

