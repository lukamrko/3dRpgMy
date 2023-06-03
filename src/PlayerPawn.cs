using System.Linq;
using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class PlayerPawn : APawn
{
	public bool Skipped = false;
	public bool IsTotem = false;	

	public override void _Ready()
	{
		Character = GetNode<Sprite3D>("Character");
		AnimationTree = GetNode<AnimationTree>("Character/AnimationTree");
		CharacterStats = GetNode<Node3D>("CharacterStats");
		// CharacterStats.Visible=true;
        HealthLabel = CharacterStats.GetNode<Label>("Health/SubViewport/Label");
        NameLabel = CharacterStats.GetNode<Label>("Name/SubViewport/Label");
		CurrTile = GetNode<RayCast3D>("Tile");

        SoundPawnAttack = GetNode<AudioStreamPlayer>("SoundPawnAttack");

        LoadStats();
		LoadAnimatorSprite();
		DisplayPawnStats(true);
		
		if(PawnClass == PawnClass.Totem)
		{
			IsTotem = true;
		}

        deadZone = GetParent().GetParent().GetNode<Area3D>("Arena/DeadZone");
        deadZoneDetector = GetNode<Area3D>("DeadZoneDetector");
        deadZoneDetector.AreaEntered += (deadZone) => AreaDetection(deadZone);
		if(PawnClass == PawnClass.Totem)
		{
			CharacterStats.Translate(Vector3.Up*1);
		}
	}

    public void AreaDetection(Area3D area)
    {
        GD.Print("Big fall");
        DealDirectDamageAndRemoveIfDead(this, 999);
    }

	public bool CanAct()
	{
		return (CanMove || CanAttack )
			&& CurrHealth > 0
            && !IsTotem;
	}

	public override void TintWhenNotAbleToAct()
	{
		Character.Modulate = !CanAct()
			? new Color(0.7f, 0.7f, 0.7f)
			: new Color(1, 1, 1);
	}

	public override void _Process(double delta)
	{
		RotatePawnSprite();
		ApplyMovement(delta);
		StartAnimator();
		TintWhenNotAbleToAct();
	}

}
