using System.Diagnostics;
using Godot;
using System;

public class PlayerController : Spatial
{
    Pawn CurrentPawn = null;
    Pawn AttackablePawn = null;

    float WaitTime = 0;

    bool IsJoyStick = false;

    Arena Arena = null;
    TacticsCamera TacticsCamera = null;

    int Stage = 0;

    PlayerControllerUI UIControl;
    Godot.Collections.Array<Pawn> Pawns;

    public void Configure(Arena myArena, TacticsCamera myCamera, PlayerControllerUI myControl)
    {
        Arena = myArena;
        TacticsCamera = myCamera;
        UIControl = myControl;
        TacticsCamera.Target = Pawns[0];
        Button MoveButton = UIControl.GetAct("Move");
        Button WaitButton = UIControl.GetAct("Wait");
        Button CancelButton = UIControl.GetAct("Cancel");
        Button AttackButton = UIControl.GetAct("Attack");

        MoveButton.Connect("pressed", this, "PlayerWantsToMove");
        WaitButton.Connect("pressed", this, "PlayerWantsToWait");
        CancelButton.Connect("pressed", this, "PlayerWantsToCancel");
        AttackButton.Connect("pressed", this, "PlayerWantsToAttack");

        // UIControl.GetAct("Move").Connect("pressed", this, "PlayerWantsToMove");
        // UIControl.GetAct("Wait").Connect("pressed", this, "PlayerWantsToWait");
        // UIControl.GetAct("Cancel").Connect("pressed", this, "PlayerWantsToCancel");
        // UIControl.GetAct("Attack").Connect("pressed", this, "PlayerWantsToAttack");
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
        foreach (Pawn pawn in Pawns)
            if (pawn.CanAct())
                return true;
        return Stage > 0;
    }

    public void Reset()
    {
        foreach (Pawn pawn in Pawns)
            pawn.Reset();
    }

    public void PlayerWantsToMove()
    {
        Stage = 2;
    }

    public void PlayerWantsToCancel()
    {
        Stage = Stage > 1 ? 1 : 0;
    }

    public void PlayerWantsToWait()
    {
        CurrentPawn.DoWait();
        Stage = 0;
    }

    public void PlayerWantsToAttack()
    {
        Stage = 5;
    }

    private Pawn AuxSelectPawn()
    {
        Pawn pawn = GetMouseOverObject(2) as Pawn;
        Tile tile = pawn == null
            ? GetMouseOverObject(1) as Tile
            : pawn.GetTile();
        Arena.MarkHoverTile(tile);
        if (pawn != null)
            return pawn;
        else
        {
            if (tile != null)
                return tile.GetObjectAbove();
            else
                return null;
        }
    }

    private Tile AuxSelectTile()
    {
        Pawn pawn = GetMouseOverObject(2) as Pawn;
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
        if (Input.IsActionJustPressed("ui_accept") && CurrentPawn.CanAct() && Pawns.Contains(CurrentPawn))
        {
            TacticsCamera.Target = CurrentPawn;
            Stage = 1;
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
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.JumpHeight, Pawns);
        Arena.MarkReachableTiles(CurrentPawn.GetTile(), CurrentPawn.MoveRadius);
        Stage = 3;
    }

    public void DisplayAttackableTargets()
    {
        Arena.Reset();
        if (CurrentPawn == null)
            return;
        TacticsCamera.Target = CurrentPawn;
        Arena.LinkTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius);
        Arena.MarkAttackableTiles(CurrentPawn.GetTile(), CurrentPawn.AttackRadius);
        Stage = 6;
    }

    public void SelectNewLocation()
    {
        Tile tile = GetMouseOverObject(1) as Tile;
        Arena.MarkHoverTile(tile);
        if (Input.IsActionJustPressed("ui_accept") && tile != null && tile.Reachable)
        {
            CurrentPawn.PathStack = Arena.GeneratePathStack(tile);
            TacticsCamera.Target = tile;
            Stage = 4;
        }
    }

    public void SelectPawnToAttack()
    {
        CurrentPawn.DisplayPawnStats(true);
        if (AttackablePawn != null)
            AttackablePawn.DisplayPawnStats(false);
        Tile tile = AuxSelectTile();
        if (tile != null)
            AttackablePawn = tile.GetObjectAbove();
        else
            AttackablePawn = null;

        if (AttackablePawn != null)
            AttackablePawn.DisplayPawnStats(true);
        if (Input.IsActionJustPressed("ui_accept") && tile != null && tile.Attackable)
        {
            TacticsCamera.Target = AttackablePawn;
            Stage = 7;
        }
    }

    public void MovePawn()
    {
        CurrentPawn.DisplayPawnStats(false);
        if (CurrentPawn.PathStack.Count == 0)
            if (!CurrentPawn.CanAct())
                Stage = 0;
            else
                Stage = 1;
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
        if (!CurrentPawn.CanAct())
            Stage = 0;
        else
            Stage = 1;
    }
    #endregion

    #region Camera

    public void MoveCamera()
    {
        float h = -Input.GetActionStrength("camera_left") + Input.GetActionStrength("camera_right");
        float v = -Input.GetActionStrength("camera_forward") + Input.GetActionStrength("camera_backwards");
        TacticsCamera.MoveCamera(h, v, IsJoyStick);
    }

    public void CameraRotation()
    {
        if (Input.IsActionJustPressed("camera_rotate_left"))
            TacticsCamera.YRot -= 90;
        if (Input.IsActionJustPressed("camera_rotate_right"))
            TacticsCamera.YRot += 90;
    }

    public void _Act(float delta)
    {
        MoveCamera();
        CameraRotation();
        UIControl.SetVisibilityOfActionsMenu(VisibilityBasedOnStage(), CurrentPawn);
        switch (Stage)
        {
            case 0:
                SelectPawn();
                break;
            case 1:
                DisplayAvailableActionsForPawn();
                break;
            case 2:
                DisplayAvailableMovements();
                break;
            case 3:
                SelectNewLocation();
                break;
            case 4:
                MovePawn();
                break;
            case 5:
                DisplayAttackableTargets();
                break;
            case 6:
                SelectPawnToAttack();
                break;
            case 7:
            default:
                AttackPawn(delta);
                break;
        }

    }

    private bool VisibilityBasedOnStage()
    {
        int[] StagesAsInt = new int[] { 1, 2, 3, 5, 6 };
        for (int i = 0; i < StagesAsInt.Length; i++)
        {
            if (Stage == StagesAsInt[i])
                return true;
        }
        return false;

    }

    #endregion
    public override void _Ready()
    {
        Pawns = GetChildren().As<Pawn>();

    }

    public override void _Process(float delta)
    {
    }

    public override void _Input(InputEvent @event)
    {
        IsJoyStick = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;
        UIControl.IsJoyStick = IsJoyStick;
    }


}
