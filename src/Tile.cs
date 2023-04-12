using Godot;
using System;

public partial class Tile : StaticBody3D
{
	public Material HoverMat = Utils.CreateMaterial(new Color("33ffffff"));
	public Material ReachableMat = Utils.CreateMaterial(new Color("e6143464"));
	public Material HoverReachableMat = Utils.CreateMaterial(new Color("d92d548f"));
	public Material AttackableMat = Utils.CreateMaterial(new Color("e6b4202a"));
	public Material HoverAttackableMat = Utils.CreateMaterial(new Color("d9df3e23"));
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

	public Godot.Collections.Array<Tile> GetNeighbors(float height)
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
		AddChild(TileRaycastingTSCN.Instantiate());
		// AddChild(TileRaycastingTSCN.Instance());
		Reset();
		TileRaycasting = GetNode<TileRaycasting>("RayCasting");
	}

	public override void _Ready()
	{
		TileRaycastingTSCN = ResourceLoader.Load<PackedScene>("res://assets/tscn/TileRaycasting.tscn");
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
