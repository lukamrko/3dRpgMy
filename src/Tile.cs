using Godot;
using System;

public class Tile : StaticBody
{

    public Material HoverMat = Utils.CreateMaterial(new Color("33ffffff"));
    public Material ReachableMat = Utils.CreateMaterial(new Color("e6143464"));
    public Material HoverReachableMat = Utils.CreateMaterial(new Color("d92d548f"));
    public Material AttackableMat = Utils.CreateMaterial(new Color("e6b4202a"));
    public Material HoverAttackableMat = Utils.CreateMaterial(new Color("d9df3e23"));
    MeshInstance CurrTiles;

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

    public Godot.Collections.Array<Tile> GetNeighbors(float height)
    {
        // TileRaycasting rayCasting = GetNode<TileRaycasting>("RayCasting");
        // return rayCasting.GetAllNeighbors(height);

        return TileRaycasting.GetAllNeighbors(height);
    }

    public object GetObjectAbove()
    {
        // TileRaycasting rayCasting = GetNode<TileRaycasting>("RayCasting");
        // return rayCasting.GetObjectAbove() as Pawn;
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
        AddChild(TileRaycastingTSCN.Instance());
        Reset();
        TileRaycasting = GetNode<TileRaycasting>("RayCasting");
    }



    public override void _Ready()
    {
        // var TileRaycastingHelper = ResourceLoader.Load<PackedScene>("res://assets/tscn/TileRaycasting.tscn");
        // TileRaycastingTSCN = TileRaycastingHelper.Instance();
        TileRaycastingTSCN = ResourceLoader.Load<PackedScene>("res://assets/tscn/TileRaycasting.tscn");
        // TileRaycasting = GetNode<TileRaycasting>("RayCasting");


        CurrTiles = GetNode<MeshInstance>("Tile");
        // try
        // {
        // }
        // catch (InvalidCastException e)
        // {
        //     GD.Print("GRRRR nisan naša");
        //     GD.Print(e);
        //     GD.Print(e.Message);
        // }
    }

    public override void _Process(float delta)
    {
        CurrTiles.Visible = Attackable || Reachable || Hover;
        switch(Hover)
        {
            case true:
                if(Reachable)
                    CurrTiles.MaterialOverride = HoverReachableMat;
                else if (Attackable)
                    CurrTiles.MaterialOverride = HoverAttackableMat;
                else
                    CurrTiles.MaterialOverride = HoverMat;
                break;
            case false:
                if(Reachable)
                    CurrTiles.MaterialOverride = ReachableMat;
                else if (Attackable)
                    CurrTiles.MaterialOverride = AttackableMat;
                break;
        }
    }
}
