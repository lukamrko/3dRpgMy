using System.Linq;
using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class EnemyPawn : APawn
{
    //side which attacks, plus distance
    public KeyValuePair<int, WorldSide> AttackingTowards;
    public EnemyPhase CurrentPhase = EnemyPhase.NotAnEnemyPhase;

    public Vector3 _oldPosition;

    public override void _Ready()
    {
        Character = GetNode<Sprite3D>("Character");
        AnimationTree = GetNode<AnimationTree>("Character/AnimationTree");
        CharacterStats = GetNode<Node3D>("CharacterStats");
        var healthSubViewPort = CharacterStats.GetNode<SubViewport>("Health/SubViewport");
        healthSubViewPort.RenderTargetUpdateMode = SubViewport.UpdateMode.WhenParentVisible;
        HealthLabel = healthSubViewPort.GetNode<Label>("Label");

        var nameSubViewPort = CharacterStats.GetNode<SubViewport>("Name/SubViewport");
        nameSubViewPort.RenderTargetUpdateMode = SubViewport.UpdateMode.WhenParentVisible;
        NameLabel = nameSubViewPort.GetNode<Label>("Label");
        CurrTile = GetNode<RayCast3D>("Tile");

        SoundPawnAttack = GetNode<AudioStreamPlayer>("SoundPawnAttack");
        deadZone = GetParent().GetParent().GetNode<Area3D>("Arena/DeadZone");
        deadZoneDetector = GetNode<Area3D>("DeadZoneDetector");

        SoundPawnAttack = GetNode<AudioStreamPlayer>("SoundPawnAttack");

        deadZoneDetector.AreaEntered += (deadZone) => AreaDetection(deadZone);
        this.Visible = true;
        LoadStats();
        LoadAnimatorSprite();
        DisplayPawnStats(true);
        _oldPosition = this.Position;
        GD.Print("Name label text:" + NameLabel.Text);
        GD.Print("Health label text:" + HealthLabel.Text);
    }

    public void AreaDetection(Area3D area)
    {
        GD.Print("Big fall");
        DealDirectDamageAndRemoveIfDead(this, 999);
    }

    public bool EnemyCanFirstAct()
    {
        return CanMove;
    }
    public bool EnemyCanSecondAct()
    {
        return CanAttack;
    }

    public override void _Process(double delta)
    {
        RotatePawnSprite();
        ApplyMovement(delta);
        StartAnimator();
        TintWhenNotAbleToAct();
    }

    private void DebugHelper()
    {
        var newPosition = this.Position;
        if (!_oldPosition.Equals(newPosition))
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


