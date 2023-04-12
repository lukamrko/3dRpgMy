using System.Collections.Generic;
using Godot;
using System;

public partial class TileRaycasting : Node3D
{
    private Node3D _neighbors;
    private RayCast3D _above;
    private Godot.Collections.Array<RayCast3D> _neighborRayCasts;
    private readonly Dictionary<WorldSide, string> worldSideToString;

    public TileRaycasting()
    {
        _Ready();
        worldSideToString = new Dictionary<WorldSide, string>
        {
            { WorldSide.North, "N"},
            { WorldSide.East, "E"},
            { WorldSide.West, "W"},
            { WorldSide.South, "S"},
        };
    }

    public override void _Ready()
    {
        _neighbors = GetNode<Node3D>("Neighbors");
        _above = GetNode<RayCast3D>("Above");
        _neighborRayCasts = new Godot.Collections.Array<RayCast3D>();
    }

    public Godot.Collections.Array<Tile> GetAllNeighbors(float height)
    {
        FetchNeighborRayCastsIfEmpty();
        Godot.Collections.Array<Tile> tileNeighbors = new Godot.Collections.Array<Tile>();
        foreach (RayCast3D rayCast in _neighborRayCasts)
        {
            Tile obj = rayCast.GetCollider() as Tile;
            Tile parent = GetParent() as Tile;
            if (parent is null || obj is null)
            {
                continue;
            }
            bool objectFulfillsYAxis = Math.Abs(obj.Position.Y - parent.Position.Y) <= height;
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
        _neighborRayCasts = _neighbors.GetChildren().As<RayCast3D>();
    }

    public Tile GetSpecificNeighbor(WorldSide worldSide)
    {
        FetchNeighborRayCastsIfEmpty();
        var rayCastName = worldSideToString[worldSide];
        var rayCast = new RayCast3D();
        foreach(RayCast3D ray in _neighborRayCasts)
        {
            if(ray.Name.Equals(rayCastName))
            {
                rayCast = ray;
                break;
            }
        }
        if(rayCast is null)
        {
            GD.Print("Something went very wrong");
            return null;
        }

        Tile obj = rayCast.GetCollider() as Tile;
        GD.Print(string.Format(@"One tile that we searched for had value: {0}", obj is object));
        return obj;
    }

    public APawn GetObjectAbove()
    {
        APawn pawn = _above.GetCollider() as APawn;
        return pawn;
    }

}