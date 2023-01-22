using Godot;
using System;

public class TileRaycasting : Spatial
{
    private Spatial _neighbors;
    private RayCast _above;

    public override void _Ready()
    {
        _neighbors = GetNode<Spatial>("Neighbors");
        _above = GetNode<RayCast>("Above");
    }

    public Godot.Collections.Array<Tile> GetAllNeighbors(float height)
    {
        Godot.Collections.Array<Tile> objects = new Godot.Collections.Array<Tile>();
        foreach (Node ray in _neighbors.GetChildren())
        {
            RayCast rayCast = ray as RayCast;
            Tile obj = rayCast.GetCollider() as Tile; //Those might be some sort of collision since they end like tile093_col
            Tile parent = GetParent() as Tile;
            if (parent == null)
                continue;
            bool objectFulfillsYAxis = Math.Abs(obj.Translation.y - parent.Translation.y) <= height;
            if (obj != null && objectFulfillsYAxis)
                objects.Add(obj);
        }
        return objects;
    }

    public Pawn GetObjectAbove()
    {
        return _above.GetCollider() as Pawn;
    }



}
