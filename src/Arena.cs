using System.Collections.Generic;
using System.Linq;
using Godot;
using System;

public partial class Arena : Node3D
{
    Godot.Collections.Array<Tile> TilesChildren;
    Dictionary<Vector3, Tile> RoundDownedTilesDictionary = new Dictionary<Vector3, Tile>();

    public void LinkTiles<[MustBeVariant] T>(Tile root, float height, Godot.Collections.Array<T> allies = null) where T : APawn
    {
        Godot.Collections.Array<Tile> tiles = new Godot.Collections.Array<Tile> { root };
        while (tiles.Count > 0)
        {
            Tile currentTile = tiles[0];
            tiles.RemoveAt(0);
            var neighbors = currentTile.GetNeighbors(height).Values;
            foreach (Tile neighbor in neighbors)
            {
                var neighborRootState = neighbor.Root is null && neighbor != root;
                var pawnsOccupy = neighbor.IsTaken()
                    && allies is object
                    && !allies.Contains(neighbor.GetObjectAbove() as APawn);
                var shouldLinkTiles = neighborRootState && !pawnsOccupy;

                if (shouldLinkTiles)
                {
                    neighbor.Root = currentTile;
                    neighbor.Distance = currentTile.Distance + 1;
                    tiles.Add(neighbor);
                }
            }
        }
    }

    public void MarkHoverTile(Tile markedTile)
    {
        foreach (var tile in TilesChildren)
        {
            tile.Hover = false;
        }

        if (markedTile is object)
        {
            markedTile.Hover = true;
        }
    }

    public void MarkReachableTiles(Tile root, int distance)
    {
        foreach (var tile in TilesChildren)
        {
            tile.Reachable = tile.Distance > 0 && tile.Distance <= distance && !tile.IsTaken() || tile.Equals(root);
        }
    }

    public void MarkAttackableTiles(Tile root, float distance)
    {
        foreach (var tile in TilesChildren)
        {
            tile.Attackable = tile.Distance > 0 && tile.Distance <= distance || tile.Equals(root);
        }
    }

    /// <summary>
    /// Thy shall pass rounded location to this method
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public Tile GetTileAtLocation(Vector3 location)
    {
        Tile tile = null;
        RoundDownedTilesDictionary.TryGetValue(location, out tile);
        if (tile is object)
        {
            GD.Print("The madman actually found the tile");
        }

        return tile;
    }


    public Godot.Collections.Array<Vector3> GeneratePathStack(Tile to)
    {
        Godot.Collections.Array<Vector3> pathStack = new Godot.Collections.Array<Vector3>();
        while (to is object)
        {
            pathStack.Insert(0, to.GlobalTransform.Origin);
            to = to.Root;
        }
        return pathStack;
    }

    public Godot.Collections.Array<Vector3> GenerateSimplePathStack(Tile from, Tile to)
    {
        Godot.Collections.Array<Vector3> pathStack = new Godot.Collections.Array<Vector3>();
        pathStack.Add(from.GlobalTransform.Origin);
        pathStack.Add(to.GlobalTransform.Origin);

        return pathStack;
    }

    public void Reset()
    {
        foreach (var tile in TilesChildren)
        {
            tile.Reset();
        }
    }

    public override void _Ready()
    {
        var Tiles = GetNode<Node3D>("Tiles");
        Tiles.Visible = true;
        Utils.ConvertTilesIntoStaticBodies(Tiles);
        TilesChildren = GetNode("Tiles").GetChildren().As<Tile>();
        GenerateDictionaryForTiles();
    }

    private void GenerateDictionaryForTiles()
    {
        foreach (Tile Tile in TilesChildren)
        {
            //TODO This seems like a way to refactor this, but not to sure
            // var tileLocation = Tile.GlobalTranslation.Rounded();
            var tileLocation = Tile.GlobalPosition.Rounded();
            RoundDownedTilesDictionary.Add(tileLocation, Tile);
        }
    }


    private const int maxIterationOfGetNearestNeighborTileToPawn = 3;
    public Tile GetNearestNeighborTileToPawn(int distance, APawn pawn, Godot.Collections.Array<PlayerPawn> pawns, int currentIteration = 1)
    {
        Tile nearestTile = null;
        if (pawn.PawnStrategy == PawnStrategy.ObjectiveSniper)
        {
            pawns = GetOnlyTotems(pawns);
        }

        //Try to get free tiles directly close to the pawn
        foreach (PlayerPawn _pawn in pawns)
        {
            var currentPawnTile = _pawn.GetTile();
            var tiles = currentPawnTile.GetNeighbors(pawn.JumpHeight);
            nearestTile = SpecificDistanceAwayTiles(distance, nearestTile, tiles, pawn);
        }

        //If it isn't reachable try to move as close as possible
        while (nearestTile is object
            && !nearestTile.Reachable)
        {
            nearestTile = nearestTile.Root;
        }

        if (nearestTile is object)
        {
            return nearestTile;
        }


        //If didn't find any tiles try once again but for tiles that are distance+1 tiles away from player
        if (nearestTile is null
            && ++currentIteration <= maxIterationOfGetNearestNeighborTileToPawn)
        {
            return GetNearestNeighborTileToPawn(++distance, pawn, pawns, currentIteration: currentIteration);
            // foreach (PlayerPawn _pawn in pawns)
            // {
            //     var currentPawnTile = _pawn.GetTile();
            //     var tiles = currentPawnTile.GetNeighbors(pawn.JumpHeight);
            //     nearestTile = SpecificDistanceAwayTiles(2, nearestTile, tiles, pawn);
            // }

            // while (nearestTile is object
            //     && !nearestTile.Reachable)
            // {
            //     nearestTile = nearestTile.Root;
            // }

            // if (nearestTile is object)
            // {
            //     return nearestTile;
            // }
        }

        return pawn.GetTile();
    }

    private Godot.Collections.Array<PlayerPawn> GetOnlyTotems(Godot.Collections.Array<PlayerPawn> pawns)
    {
        var totems = new Godot.Collections.Array<PlayerPawn>();
        for(int i=0; i<pawns.Count; i++)
        {
            if(pawns[i].IsTotem)
            {
                totems.Add(pawns[i]);
            }
        }
        return totems;
    }

    private Tile OneTileAwayTiles(Tile nearestTile, Dictionary<WorldSide, Tile> targetTileNeighbors, APawn pawn)
    {
        var tiles = targetTileNeighbors.Select(x => x.Value).ToList();
        foreach (Tile newTile in tiles)
        {
            var isNewTileNearestTile = (nearestTile is null || newTile.Distance < nearestTile.Distance)
                && newTile.Distance >= 0
                && !newTile.IsTaken(pawn);

            if (isNewTileNearestTile)
            {
                nearestTile = newTile;
            }
        }
        return nearestTile;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="distance">Distance from target tile, counting first neighbor</param>
    /// <param name="nearestTile"></param>
    /// <param name="targetTileNeighbors"></param>
    /// <returns></returns>
    private Tile SpecificDistanceAwayTiles(int distance, Tile nearestTile, Dictionary<WorldSide, Tile> targetTileNeighbors, APawn pawn)
    {
        if (distance == 1)
        {
            return OneTileAwayTiles(nearestTile, targetTileNeighbors, pawn);
        }

        var tiles = new Godot.Collections.Array<Tile>();
        foreach (var tile in targetTileNeighbors)
        {
            var potentialTile = new Tile();
            var currentTile = tile.Value;
            var tileSide = tile.Key;
            for (int i = 0; i < distance - 1; i++)
            {
                currentTile = currentTile.GetNeighborAtWorldSide(tileSide);
                if (currentTile is null)
                {
                    break;
                }
            }
            if (currentTile is object)
            {
                tiles.Add(currentTile);
            }
        }

        // var tiles = targetTileNeighbors.Select(x => x.Value).ToList();
        foreach (Tile newTile in tiles)
        {
            var isNewTileNearestTile = (nearestTile is null || newTile.Distance < nearestTile.Distance)
                && newTile.Distance > 0
                && !newTile.IsTaken(pawn);

            if (isNewTileNearestTile)
            {
                nearestTile = newTile;
            }
        }
        return nearestTile;
    }

    public PlayerPawn GetWeakestPawnToAttack(Godot.Collections.Array<PlayerPawn> pawns)
    {
        PlayerPawn weakest = null;
        foreach (PlayerPawn pawn in pawns)
        {
            var isPawnWeakestPawn = (weakest is null || pawn.CurrHealth < weakest.CurrHealth)
                && pawn.CurrHealth > 0
                && pawn.GetTile().Attackable;

            if (isPawnWeakestPawn)
            {
                weakest = pawn;
            }
        }
        return weakest;
    }

    public PlayerPawn GetRandomPawnToAttack(Godot.Collections.Array<PlayerPawn> pawns)
    {
        Godot.Collections.Array<PlayerPawn> potentiallyAttackablePawns = new Godot.Collections.Array<PlayerPawn>();
        foreach (PlayerPawn pawn in pawns)
        {
            var pawnTile = pawn.GetTile();
            if (pawn.CurrHealth > 0 && pawnTile.Attackable)
            {
                potentiallyAttackablePawns.Add(pawn);
            }
        }

        if (potentiallyAttackablePawns.Count > 0)
        {
            return potentiallyAttackablePawns.GetRandom();
        }

        return null;
    }

    // public PlayerPawn GetRandomPawnToAttack(Godot.Collections.Array<PlayerPawn> pawns)
    // {
    //     PlayerPawn randomPawn = pawns.
    //     PlayerPawn weakest = null;
    //     foreach (PlayerPawn pawn in pawns)
    //     {
    //         if ((weakest is null || pawn.CurrHealth < weakest.CurrHealth) && pawn.CurrHealth > 0 && pawn.GetTile().Attackable)
    //             weakest = pawn;
    //     }
    //     return weakest;
    // }
}
