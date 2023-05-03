using System.Collections.Generic;
using Godot;

public class ForcedMovement
{
    private APawn CurrentPawn;
    private Arena Arena;
    private TacticsCamera TacticsCamera;
    public ForcedMovement(APawn currentPawn, Arena arena, TacticsCamera tacticsCamera)
    {
        CurrentPawn = currentPawn;
        Arena = arena;
        TacticsCamera = tacticsCamera;
    }

    #region  OutsideForces
    private ForceCalculation _forceCalculation = ForceCalculation.ForceFree;
    public bool ShouldApplyForce(IEnumerable<APawn> pawns)
    {
        if (_forceCalculation == ForceCalculation.ForceBeingApplied
            && CurrentPawn is object)
        {
            ApplyForce();
            return true;
        }
        foreach (var pawn in pawns)
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
        if (CurrentPawn.PathStack.Count == 0)
        {
            _forceCalculation = ForceCalculation.ForceFree;
            CurrentPawn.shouldBeForciblyMoved = false;
        }
    }
    #endregion

}
