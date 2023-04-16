using System.Linq;
using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class EnemyPawn : APawn
{
	public Vector3? AttackingTowards;
	public EnemyPhase CurrentPhase = EnemyPhase.NotAnEnemyPhase;

	public Vector3 _oldPosition;
	public override void _Ready()
	{
		Character = GetNode<Sprite3D>("Character");
		AnimationTree = GetNode<AnimationTree>("Character/AnimationTree");
		CharacterStats = GetNode<Node3D>("CharacterStats");
		HealthLabel = GetNode<Label>("CharacterStats/Health/SubViewport/Label");
		NameLabel = GetNode<Label>("CharacterStats/Name/SubViewport/Label");
		CurrTile = GetNode<RayCast3D>("Tile");
		LoadStats();
		LoadAnimatorSprite();
		DisplayPawnStats(true);
		_oldPosition = this.Position;
	}

    public bool EnemyCanFirstAct()
	{
		return CanMove && CurrHealth > 0;
	}
	public bool EnemyCanSecondAct()
	{
		return CanAttack && CurrHealth > 0;
	}

	public override void _Process(double delta)
	{
		RotatePawnSprite();
		ApplyMovement(delta);
		StartAnimator();
		TintWhenNotAbleToAct();
		HealthLabel.Text = CurrHealth.ToString() + "/" + MaxHealth.ToString();
        DebugHelper();
    }

    private void DebugHelper()
    {
		var newPosition = this.Position;
		if(!_oldPosition.Equals(newPosition))
		{
			GD.Print("Position has changed");
		}
		_oldPosition = newPosition;
    }

	public override void TintWhenNotAbleToAct()
	{
		var canAct = false;
		switch (CurrentPhase)
		{
			case EnemyPhase.NotAnEnemyPhase:
				return;
			case EnemyPhase.FirstPhase:
				canAct = EnemyCanFirstAct();
				break;
			case EnemyPhase.SecondPhase:
				canAct = EnemyCanSecondAct();
				break;
		}
		ModulateCharacter(canAct);
	}

	private void ModulateCharacter(bool canAct)
	{
		Character.Modulate = !canAct
			? new Color(0.7f, 0.7f, 0.7f)
			: new Color(1, 1, 1);
	}
	
}


