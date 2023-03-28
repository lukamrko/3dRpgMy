using System.Collections.Generic;
using System.Linq;
using Godot;
using System;

public class Arena : Spatial
{
    Godot.Collections.Array<Tile> TilesChildren;
    Dictionary<Vector3, Tile> RoundDownedTilesDictionary = new Dictionary<Vector3, Tile>();

    public void LinkTiles<T>(Tile root, float height, Godot.Collections.Array<T> allies = null) where T : APawn
    {
        Godot.Collections.Array<Tile> tiles = new Godot.Collections.Array<Tile> { root };
        while (tiles.Count > 0)
        {
            Tile currentTile = tiles[0];
            tiles.RemoveAt(0);
            var neighbors = currentTile.GetNeighbors(height);
            foreach (Tile neighbor in neighbors)
            {
                var neighborRootState = neighbor.Root is null && neighbor != root;
                var pawnsOccupy = neighbor.IsTaken()
                    && allies is object
                    && !allies.Contains(neighbor.GetObjectAbove() as APawn);
                var shouldLinkTiles = neighborRootState && !pawnsOccupy;

                if(shouldLinkTiles)
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
        if(tile is object)
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
            pathStack.Insert(0, to.GlobalTransform.origin);
            to = to.Root;
        }
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
        var Tiles = GetNode<Spatial>("Tiles");
        Tiles.Visible = true;
        Utils.ConvertTilesIntoStaticBodies(Tiles);
        TilesChildren = GetNode("Tiles").GetChildren().As<Tile>();
        GenerateDictionaryForTiles();
    }

    private void GenerateDictionaryForTiles()
    {
        foreach(Tile Tile in TilesChildren)
        {
            var tileLocation = Tile.GlobalTranslation.Rounded();
            RoundDownedTilesDictionary.Add(tileLocation, Tile);
        }
    }

    public Tile GetNearestNeighborToPawn(APawn pawn, Godot.Collections.Array<PlayerPawn> pawns)
    {
        Tile nearestTile = null;
        foreach (PlayerPawn _pawn in pawns)
        {
            if (_pawn.CurrHealth <= 0)
            {
                continue;
            }
            var currentPawnTile = _pawn.GetTile();
            var tiles = currentPawnTile.GetNeighbors(pawn.JumpHeight);
            foreach (Tile newTile in tiles)
            {
                var isNewTileNearestTile = (nearestTile is null || newTile.Distance < nearestTile.Distance) 
                    && newTile.Distance > 0 
                    && !newTile.IsTaken();

                if (isNewTileNearestTile)
                {
                    nearestTile = newTile;
                }
            }
        }

        while (nearestTile is object 
            && !nearestTile.Reachable)
        {
            nearestTile = nearestTile.Root;
        }
        
        if (nearestTile is object)
        {
            return nearestTile;
        }

        return pawn.GetTile();
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
