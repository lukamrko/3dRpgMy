using System.Linq;
using Godot;
using System;
using System.Collections.Generic;

public class PlayerPawn : APawn
{
    public bool Skipped = false;

    public override void _Ready()
    {
        Character = GetNode<Sprite3D>("Character");
        AnimationTree = GetNode<AnimationTree>("Character/AnimationTree");
        CharacterStats = GetNode<Spatial>("CharacterStats");
        HealthLabel = GetNode<Label>("CharacterStats/Health/Viewport/Label");
        NameLabel = GetNode<Label>("CharacterStats/Name/Viewport/Label");
        CurrTiles = GetNode<RayCast>("Tile");
        LoadStats();
        LoadAnimatorSprite();
        DisplayPawnStats(true);
    }

    public bool CanAct()
    {
        return (CanMove || CanAttack)
            && CurrHealth > 0;
    }

    public override void TintWhenNotAbleToAct()
    {
        Character.Modulate = !CanAct()
            ? new Color(0.7f, 0.7f, 0.7f)
            : new Color(1, 1, 1);
    }

    public override void _Process(float delta)
    {
        RotatePawnSprite();
        ApplyMovement(delta);
        StartAnimator();
        TintWhenNotAbleToAct();
        HealthLabel.Text = CurrHealth.ToString() + "/" + MaxHealth.ToString();
    }

}
