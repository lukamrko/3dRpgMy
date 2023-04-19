// public class ForcedMovement
// {
//     private ForceCalculation _forceCalculation = ForceCalculation.ForceFree;
//     public bool ShouldApplyForce(Godot.Collections.Array<APawn> Pawns)
//     {
//         if (_forceCalculation == ForceCalculation.ForceBeingApplied)
//         {
//             ApplyForce();
//             return true;
//         }
//         foreach (var pawn in Pawns)
//         {
//             if (pawn.shouldBeForciblyMoved
//                 && _forceCalculation != ForceCalculation.ForceBeingApplied)
//             {
//                 oldStage = Stage;
//                 Stage = EnemyStage.ForceBeingCalculated;
//                 CurrentPawn = pawn;
//                 return true;
//             }
//         }
//         return false;
//     }

//     public void DoForcedMovement(double delta)
//     {
//         if (_forceCalculation == ForceCalculation.ForceBeingCalculated)
//         {
//             CalculateForce();
//             ApplyForce();
//         }
//         else
//         {
//             ApplyForce();
//         }
//     }

//     private void CalculateForce()
//     {
//         var location = (CurrentPawn.GlobalPosition + CurrentPawn.directionOfForcedMovement).Rounded();
//         Tile to = Arena.GetTileAtLocation(location);
//         if (to is null)
//         {
//             CurrentPawn.Velocity = CurrentPawn.directionOfForcedMovement;
//             CurrentPawn.MoveAndSlide();
//         }
//         else
//         {
//             var currentPawnTile = CurrentPawn.GetTile();
//             CurrentPawn.PathStack = Arena.GenerateSimplePathStack(currentPawnTile, to);
//             // CurrentPawn.PathStack = Arena.GeneratePathStack(to);
//             TacticsCamera.Target = to;
//         }
//         Stage = EnemyStage.ForceBeingApplied;
//     }

//     private void ApplyForce()
//     {
//         GD.Print("I entered apply force method");
//         if (CurrentPawn.PathStack.Count == 0)
//         {
//             Stage = oldStage;
//             CurrentPawn.shouldBeForciblyMoved = false;
//         }
//     }

// }
