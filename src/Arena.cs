using System.Linq;
using Godot;
using System;

public class Arena : Spatial
{
    Godot.Collections.Array<StaticBody> TilesChildren;

    public Arena()
    {
        // _Ready();
    }

    public void LinkTiles(Tile root, float height, Godot.Collections.Array<PlayerPawn> allies = null)
    {
        Godot.Collections.Array<Tile> pq = new Godot.Collections.Array<Tile> { root };
        while (pq.Count > 0)
        {
            Tile currentTile = pq[0];
            pq.RemoveAt(0);
            foreach (Tile neighbor in currentTile.GetNeighbors(height))
            {
                if (neighbor.Root == null && neighbor != root)
                {
                    if (!(neighbor.IsTaken() 
                        && allies != null 
                        && !allies.Contains(neighbor.GetObjectAbove() as APawn)))
                    {
                        neighbor.Root = currentTile;
                        neighbor.Distance = currentTile.Distance + 1;
                        pq.Add(neighbor);
                    }
                }
            }
        }
    }

    public void MarkHoverTile(Tile tile)
    {
        foreach (var tHelp in TilesChildren)
        {
            Tile t = tHelp as Tile;
            t.Hover = false;
        }
        if (tile != null)
            tile.Hover = true;
    }

    public void MarkReachableTiles(Tile root, int distance)
    {
        foreach (var tHelp in TilesChildren)
        {
            Tile t = tHelp as Tile;
            t.Reachable = t.Distance > 0 && t.Distance <= distance && !t.IsTaken() || t.Equals(root);
        }
    }

    public void MarkAttackableTiles(Tile root, float distance)
    {
        foreach (var tHelp in TilesChildren)
        {
            Tile t = tHelp as Tile;
            t.Attackable = t.Distance > 0 && t.Distance <= distance || t.Equals(root);
        }
    }

    public Godot.Collections.Array<Vector3> GeneratePathStack(Tile to)
    {
        Godot.Collections.Array<Vector3> pathStack= new Godot.Collections.Array<Vector3>();
        while(to!=null)
        {
            pathStack.Insert(0, to.GlobalTransform.origin);
            to=to.Root;
        }
        return pathStack;
    }

    public void Reset()
    {
        foreach(var helpT in TilesChildren)
        {
            Tile t = helpT as Tile;
            t.Reset();
        }
    }

    public override void _Ready()
    {
        var Tiles = GetNode<Spatial>("Tiles");
        Tiles.Visible = true;
        Utils.ConvertTilesIntoStaticBodies(Tiles);
        TilesChildren = GetNode("Tiles").GetChildren().As<StaticBody>();
    }

    public Tile GetNearestNeighborToPawn(APawn pawn, Godot.Collections.Array<PlayerPawn> pawns)
    {
        Tile nearestTile = null;
        foreach(PlayerPawn _pawn in pawns)
        {
            if(_pawn.CurrHealth<=0)
                continue;
            foreach(Tile n in _pawn.GetTile().GetNeighbors(pawn.JumpHeight))
            {
                 if((nearestTile==null || n.Distance<nearestTile.Distance)&& n.Distance > 0 && !n.IsTaken())
                    nearestTile=n; 
            }
        }
        while(nearestTile!=null && !nearestTile.Reachable)
            nearestTile=nearestTile.Root;
        if(nearestTile!=null)
            return nearestTile;
        return pawn.GetTile();
    }

    public PlayerPawn GetWeakestPawnToAttack(Godot.Collections.Array<PlayerPawn> pawns)
    {
        PlayerPawn weakest = null;
        foreach(PlayerPawn pawn in pawns)
        {
            if((weakest==null  || pawn.CurrHealth<weakest.CurrHealth) && pawn.CurrHealth>0 && pawn.GetTile().Attackable)
                weakest=pawn;
        }
        return weakest;
    }
}
