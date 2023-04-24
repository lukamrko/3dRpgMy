using Godot;
using System;

public partial class DeathZone : Area3D
{
	public override void _Ready()
	{
		// this.BodyEntered += OnBodyEntered;
	}

	public void OnBodyEntered(Node3D body)
	{
		GD.Print("Dead zone");
		if (body is APawn pawn)
		{
			pawn.DealDirectDamageAndRemoveIfDead(pawn, 999);
		}
	}


}
