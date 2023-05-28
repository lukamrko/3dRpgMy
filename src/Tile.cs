using System.Collections.Generic;
using Godot;
using System;

public partial class Tile : StaticBody3D
{
    // public Material HoverMat = Utils.CreateMaterial(new Color("33ffffff"));
    // public Material ReachableMat = Utils.CreateMaterial(new Color("e6143464"));
    // public Material HoverReachableMat = Utils.CreateMaterial(new Color("d92d548f"));
    // public Material AttackableMat = Utils.CreateMaterial(new Color("e6b4202a"));
    // public Material HoverAttackableMat = Utils.CreateMaterial(new Color("d9df3e23"));
    public Material HoverMat = ResourceLoader.Load("res://assets/textures/tileHoverMat.tres") as Material;
    public Material ReachableMat = ResourceLoader.Load("res://assets/textures/tileReachableMat.tres") as Material;
	public Material HoverReachableMat = ResourceLoader.Load("res://assets/textures/tileHoverReachableMat.tres") as Material;
    public Material AttackableMat = ResourceLoader.Load("res://assets/textures/tileAttackableMat.tres") as Material;
    public Material HoverAttackableMat = ResourceLoader.Load("res://assets/textures/tileHoverAttackableMat.tres") as Material;

    MeshInstance3D CurrTiles;

	public Tile Root;
	public int Distance;

	public bool Reachable = false;
	public bool Attackable = false;
	public bool Hover = false;
	PackedScene TileRaycastingTSCN;
	TileRaycasting TileRaycasting;

	public Tile()
	{
		_Ready();
	}

	public Tile GetNeighborAtWorldSide(WorldSide side)
	{
		var tile = TileRaycasting.GetSpecificNeighbor(side);
		return tile;
	}

	public Vector3 GetNeighborPositionAtWorldSide(WorldSide side)
	{
		var position = new Vector3(Position.X, -99, Position.Z);
		switch(side)
		{
			case WorldSide.North:
                position.Z -= APawn.DistanceBetweenTiles; 
				break;
			case WorldSide.West:
                position.X -= APawn.DistanceBetweenTiles;
                break;
			case WorldSide.South:
                position.Z += APawn.DistanceBetweenTiles;
                break;
			case WorldSide.East:
                position.X += APawn.DistanceBetweenTiles;
                break;
		}
		return position;
	}

	public Dictionary<WorldSide, Tile> GetNeighbors(float height)
	{
		return TileRaycasting.GetAllNeighbors(height);
	}

	public object GetObjectAbove()
	{
		object objectAbove = TileRaycasting.GetObjectAbove();
		return objectAbove;
	}

	public bool IsTaken()
	{
		return GetObjectAbove() is object;
	}

	//Is taken excluding given pawn
    public bool IsTaken(APawn pawn)
    {
		var objectAbove = GetObjectAbove();
		if(objectAbove is APawn possiblePawn)
		{
			return (!possiblePawn.Equals(pawn));
		}
        return false;
    }

	public void Reset()
	{
		this.Root = null;
		this.Distance = 0;
		this.Reachable = false;
		this.Attackable = false;
	}

	public void ConfigureTile()
	{
		this.Hover = false;
        //TODO check if this is the way to do it
        if (TileRaycastingTSCN is null)
        {
            GD.Print("TileRaycastingTSCN  null");
        }
		var newNode = TileRaycastingTSCN.Instantiate();
		if (newNode is null)
		{
			GD.Print("new node null");
		}

		AddChild(newNode);
		// AddChild(TileRaycastingTSCN.Instance());
		Reset();
		TileRaycasting = GetNode<TileRaycasting>("RayCasting");
	}

	public override void _Ready()
	{
		TileRaycastingTSCN = GD.Load<PackedScene>("res://assets/tscn/TileRaycasting.tscn");
		CurrTiles = GetNode<MeshInstance3D>("Tile");
	}

	public override void _Process(double delta)
	{
		CurrTiles.Visible = Attackable || Reachable || Hover;
		switch (Hover)
		{
			case true:
				if (Reachable)
				{
					CurrTiles.MaterialOverride = HoverReachableMat;
				}
				else if (Attackable)
				{
					CurrTiles.MaterialOverride = HoverAttackableMat;
				}
				else
				{
					CurrTiles.MaterialOverride = HoverMat;
				}
				break;
			case false:
				if (Reachable)
				{
					CurrTiles.MaterialOverride = ReachableMat;
				}
				else if (Attackable)
				{
					CurrTiles.MaterialOverride = AttackableMat;
				}
				break;
		}
	}

}
