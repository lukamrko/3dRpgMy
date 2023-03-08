using Godot;
using System;

public class TileRaycasting : Spatial
{
    private Spatial _neighbors;
    private RayCast _above;
    private Godot.Collections.Array<RayCast> _neighborRayCasts;

    public TileRaycasting()
    {
        _Ready();
    }

    public override void _Ready()
    {
        _neighbors = GetNode<Spatial>("Neighbors");
        _above = GetNode<RayCast>("Above");
        _neighborRayCasts = new Godot.Collections.Array<RayCast>();
    }

    public Godot.Collections.Array<Tile> GetAllNeighbors(float height)
    {
        FetchNeighborRayCastsIfEmpty();
        Godot.Collections.Array<Tile> tileNeighbors = new Godot.Collections.Array<Tile>();
        foreach (RayCast rayCast in _neighborRayCasts)
        {
            Tile obj = rayCast.GetCollider() as Tile; //Those might be some sort of collision since they end like tile093_col
            Tile parent = GetParent() as Tile;
            if (parent is null || obj is null)
            {
                continue;
            }
            bool objectFulfillsYAxis = Math.Abs(obj.Translation.y - parent.Translation.y) <= height;
            if (objectFulfillsYAxis)
            {
                tileNeighbors.Add(obj);
            }
        }
        return tileNeighbors;
    }

    /// <summary>
    /// Because of the way I instance RayCasting I can't do it on ready and so I devised this method to fetch once needed
    /// </summary>
    private void FetchNeighborRayCastsIfEmpty()
    {
        if (_neighborRayCasts.Count != 0)
        {
            return;
        }
        _neighborRayCasts = _neighbors.GetChildren().As<RayCast>();

    }

    public APawn GetObjectAbove()
    {
        APawn pawn = _above.GetCollider() as APawn;
        return pawn;
    }

}