using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public abstract class IPawn
{
    public abstract Tile GetTile();
    public abstract void RotatePawnSprite();
    public abstract void LookAtDirection(Vector3 dir);
    public abstract void FollowThePath(float delta);
    public abstract void AdjustToCenter();
    public abstract void StartAnimator();  
    public abstract void ApplyMovement(float delta);
    public abstract void DoWait();
    public abstract bool DoAttack(PlayerPawn pawn, float delta);
    public abstract void Reset();
    public abstract bool CanAct();
    public abstract bool EnemyCanFirstAct();
    public abstract bool EnemyCanSecondAct();
    public abstract void LoadStats();
    public abstract void LoadAnimatorSprite();
    public abstract void TintWhenNotAbleToAct();
    public abstract void DisplayPawnStats(bool characterStatsVisible);
}