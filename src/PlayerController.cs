using System.Diagnostics;
using Godot;
using System;
using Godot.Collections;

public class PlayerController : Spatial, IObserver
{
    private PlayerPawn _currentPawn = null;

    PlayerPawn CurrentPawn
    {
        get { return _currentPawn; }
        set
        {
            if (value is null && _currentPawn is object)
            {
                GD.Print("Thus I became null");
                ResetSkipFieldOnFriendlyPawns();
            }
            _currentPawn = value;
            
        }
    }

    private void ResetSkipFieldOnFriendlyPawns()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
            pawn.skipped = false;
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

    public PlayerController()
    {
        // _Ready();
    }

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

        MoveButton.Connect("pressed", this, "PlayerWantsToMove");
        WaitButton.Connect("pressed", this, "PlayerWantsToWait");
        CancelButton.Connect("pressed", this, "PlayerWantsToCancel");
        AttackButton.Connect("pressed", this, "PlayerWantsToAttack");
    }

    public object GetMouseOverObject(uint lmask)
    {
        if (UIControl.IsMouseHoverButton())
            return null;
        Camera camera = GetViewport().GetCamera();
        Vector2 origin = !IsJoyStick
            ? GetViewport().GetMousePosition()
            : GetViewport().Size / 2;
        Vector3 from = camera.ProjectRayOrigin(origin);
        Vector3 to = from + camera.ProjectRayNormal(origin) * 1000;
        Godot.Collections.Dictionary rayIntersections = GetWorld().DirectSpaceState.IntersectRay(from, to, null, lmask);
        if (rayIntersections.Contains("collider"))
            return rayIntersections["collider"];
        return null;
    }

    public bool CanAct()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
            if (pawn.CanAct())
                return true;
        return (int)Stage > 0;
    }

    public void Reset()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
            pawn.Reset();
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
        Tile tile = pawn == null
            ? GetMouseOverObject(1) as Tile
            : pawn.GetTile();
        Arena.MarkHoverTile(tile);
        if (pawn != null)
            return pawn;
        else
        {
            if (tile != null)
                return tile.GetObjectAbove() as PlayerPawn;
            else
                return null;
        }
    }

    private Tile AuxSelectTile()
    {
        PlayerPawn pawn = GetMouseOverObject(2) as PlayerPawn;
        Tile tile = pawn == null
            ? GetMouseOverObject(1) as Tile
            : pawn.GetTile();
        Arena.MarkHoverTile(tile);
        return tile;
    }

    #region Stages
    public void SelectPawn()
    {
        Arena.Reset();
        if (CurrentPawn != null)
            CurrentPawn.DisplayPawnStats(false);
        CurrentPawn = AuxSelectPawn();
        if (CurrentPawn == null)
            return;

        CurrentPawn.DisplayPawnStats(true);
        if (Input.IsActionJustPressed("ui_accept") && CurrentPawn.CanAct() && PlayerPawns.Contains(CurrentPawn))
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
        if (CurrentPawn == null)
            return;
        TacticsCamera.Target = CurrentPawn;
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.JumpHeight, PlayerPawns);
        Arena.MarkReachableTiles(CurrentPawn.GetTile(), CurrentPawn.MoveRadius);
        Stage = PlayerStage.SelectNewLocation;
    }

    public void DisplayAttackableTargets()
    {
        Arena.Reset();
        if (CurrentPawn == null)
            return;
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
        if (Input.IsActionJustPressed("ui_accept") && tile != null && tile.Reachable)
        {
            CurrentPawn.PathStack = Arena.GeneratePathStack(tile);
            TacticsCamera.Target = tile;
            Stage = PlayerStage.MovePawn;
        }
    }

    public void SelectPawnToAttack()
    {
        CurrentPawn.DisplayPawnStats(true);
        if (AttackablePawn != null)
            AttackablePawn.DisplayPawnStats(false);
        Tile tile = AuxSelectTile();
        if (tile != null)
            AttackablePawn = tile.GetObjectAbove() as APawn;
        else
            AttackablePawn = null;

        if (AttackablePawn != null)
            AttackablePawn.DisplayPawnStats(true);
        if (Input.IsActionJustPressed("ui_accept") && tile != null && tile.Attackable)
        {
            TacticsCamera.Target = AttackablePawn;
            Stage = PlayerStage.AttackPawn;
        }
    }

    public void MovePawn()
    {
        CurrentPawn.DisplayPawnStats(false);
        if (CurrentPawn.PathStack.Count == 0)
            if (!CurrentPawn.CanAct())
                Stage = PlayerStage.SelectPawn;
            else
                Stage = PlayerStage.DisplayAvailableActionsForPawn;
    }

    public void AttackPawn(float delta)
    {
        if (AttackablePawn == null)
            CurrentPawn.CanAttack = false;
        else
        {
            CurrentPawn.DoAttack(AttackablePawn, delta);
            AttackablePawn.DisplayPawnStats(false);
            TacticsCamera.Target = CurrentPawn;
        }
        AttackablePawn = null;
        if (!CurrentPawn.CanAct())
            Stage = PlayerStage.SelectPawn;
        else
            Stage = PlayerStage.DisplayAvailableActionsForPawn;
    }
    #endregion



    public void Act(float delta)
    {
        ListenShortcuts();
        UIControl.SetVisibilityOfActionsMenu(VisibilityBasedOnStage(), CurrentPawn);
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
        float h = -Input.GetActionStrength("camera_left") + Input.GetActionStrength("camera_right");
        float v = Input.GetActionStrength("camera_forward") - Input.GetActionStrength("camera_backwards");
        TacticsCamera.MoveCamera(h, v, IsJoyStick);
    }

    public void CameraRotation()
    {
        if (Input.IsActionJustPressed("camera_rotate_left"))
            TacticsCamera.YRot -= 90;
        if (Input.IsActionJustPressed("camera_rotate_right"))
            TacticsCamera.YRot += 90;
    }
    #endregion 

    private void PlayerShortcuts()
    {
        if (Input.IsActionJustPressed("ui_focus_next"))
        {
            FastGetCurrentPawn();
            if (CurrentPawn == null)
            {
                FastGetCurrentPawn();
            }
        }
    }

    private void FastGetCurrentPawn()
    {
        foreach (PlayerPawn pawn in PlayerPawns)
        {
            var currentPawnAllows = CurrentPawn is null || pawn.skipped == false;
            var isCurrentPawn = pawn.CanAct() && currentPawnAllows;
            if (isCurrentPawn)
            {
                CurrentPawn = pawn;
                CurrentPawn.skipped = true;
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
            pawn.Attach(this);
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
                return true;
        }
        return false;

    }

    public override void _Process(float delta)
    {
    }

    public override void _Input(InputEvent @event)
    {
        IsJoyStick = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;
        UIControl.IsJoyStick = IsJoyStick;
    }

    public void Update(ISubject subject)
    {
        PlayerPawn playerPawn = subject as PlayerPawn;
        PlayerPawns.Remove(playerPawn);
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