using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public interface IPawn
{
    Tile GetTile();

    void RotatePawnSprite();

    void LookAtDirection(Vector3 dir);

    void StartAnimator();

    void FollowThePath(float delta);

    void AdjustToCenter();

    void ApplyMovement(float delta);

    void DoWait();

    bool DoAttack(IPawn pawn, float delta);

    bool DoAttackOnLocation(IPawn pawn, float delta);

    void Reset();

    void LoadStats();

    void LoadAnimatorSprite();

    void DisplayPawnStats(bool characterStatsVisible);

    void TintWhenNotAbleToAct();
}