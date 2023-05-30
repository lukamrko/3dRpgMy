using System.Collections.Generic;
using Godot;
using System;

public partial class TileRaycasting : Node3D
{
    private Node3D _neighbors;
    private RayCast3D _above;
    private Godot.Collections.Array<RayCast3D> _neighborRayCasts;
    private readonly Dictionary<WorldSide, string> worldSideToString;

    private readonly Dictionary<string, WorldSide> stringToWorldSide;

    public TileRaycasting()
    {
        // _Ready();
        worldSideToString = new Dictionary<WorldSide, string>
        {
            { WorldSide.North, "N"},
            { WorldSide.East, "E"},
            { WorldSide.West, "W"},
            { WorldSide.South, "S"},
        };
        stringToWorldSide = new Dictionary<string, WorldSide>
        {
            { "N", WorldSide.North },
            { "E", WorldSide.East },
            { "W", WorldSide.West },
            { "S", WorldSide.South },
        };
    }

    public override void _Ready()
    {
        _neighbors = GetNode<Node3D>("Neighbors");
        _above = GetNode<RayCast3D>("Above");
        _neighborRayCasts = new Godot.Collections.Array<RayCast3D>();
    }

    public Dictionary<WorldSide, Tile> GetAllNeighbors(float height)
    {
        FetchNeighborRayCastsIfEmpty();
        var tileNeighbors = new Dictionary<WorldSide, Tile>();
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
                var worldSide = stringToWorldSide[rayCast.Name];
                tileNeighbors.Add(worldSide, obj);
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
        return obj;
    }

    public APawn GetObjectAbove()
    {
        APawn pawn = _above.GetCollider() as APawn;
        return pawn;
    }

}