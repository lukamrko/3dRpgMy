using System.Linq;
using Godot;
using System;
using System.Collections.Generic;

public class EnemyPawn : APawn
{
    public EnemyPhase CurrentPhase = EnemyPhase.NotAnEnemyPhase;
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
        DisplayPawnStats(false);
    }


    public bool EnemyCanFirstAct()
    {
        return (CanMove) && CurrHealth > 0;
    }
    public bool EnemyCanSecondAct()
    {
        return CanAttack && CurrHealth > 0;
    }

    public override void _Process(float delta)
    {
        RotatePawnSprite();
        ApplyMovement(delta);
        StartAnimator();
        TintWhenNotAbleToAct();
        HealthLabel.Text = CurrHealth.ToString() + "/" + MaxHealth.ToString();
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


public enum EnemyPhase
{
    FirstPhase,
    SecondPhase,
    NotAnEnemyPhase
}