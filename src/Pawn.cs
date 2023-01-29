using System.Linq;
using Godot;
using System;
using System.Collections.Generic;

public class Pawn : KinematicBody
{
    const float PI = 3.141593f;
    const float Speed = 5f;
    const int AnimationFrames = 1;
    const int MinHeightToJump = 1;
    const int GravityStrength = 7;
    const int MinTimeForAttack = 1;

    #region class infoE

    [Export]
    public PawnClass PawnClass;
    [Export]
    public PawnStrategy PawnStrategy;
    [Export]
    public string PawnName = "Trooper";

    #endregion

    #region Pawn available actions
    public bool CanMove = true;
    public bool CanAttack = true;
    #endregion

    #region Stats
    public int MoveRadius;
    public float JumpHeight;
    public int AttackRadius;
    public int AttackPower;
    public int MaxHealth = 100;
    public int CurrHealth = 100;
    #endregion

    #region Animation
    [Export]
    public int CurrFrame = 0;
    AnimationNodeStateMachinePlayback Animator = null;
    #endregion

    #region Pathfinding
    public Godot.Collections.Array<Vector3> PathStack = new Godot.Collections.Array<Vector3>();
    Vector3 MoveDirection = Vector3.Zero;
    bool IsJumping = false;
    Vector3 Gravity = Vector3.Zero;
    float WaitDelay = 0;
    #endregion

    Label HealthLabel;

    #region Node calling
    Sprite3D Character;
    Spatial CharacterStats;
    AnimationTree AnimationTree;
    Label NameLabel;
    RayCast CurrTiles;
    #endregion

    public Pawn()
    {
        // _Ready();
    }
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

    public Tile GetTile()
    {
        return CurrTiles.GetCollider() as Tile;
    }

    public void RotatePawnSprite()
    {
        Vector3 cameraForward = -GetViewport().GetCamera().GlobalTransform.basis.z;
        float dot = GlobalTransform.basis.z.Dot(cameraForward);
        Character.FlipH = GlobalTransform.basis.x.Dot(cameraForward) > 0;
        if (dot < -0.306)
            Character.Frame = CurrFrame;
        else if (dot > 0.306)
            Character.Frame = CurrFrame + 1 * AnimationFrames;
    }

    public void LookAtDirection(Vector3 dir)
    {
        Vector3 fixedDir = Math.Abs(dir.x) > Math.Abs(dir.z)
            ? dir * new Vector3(1, 0, 0)
            : new Vector3(0, 0, 1);
        float angle = Vector3.Forward.SignedAngleTo(fixedDir.Normalized(), Vector3.Up) + PI;
        Rotation = Vector3.Up * +angle;
    }

    public void FollowThePath(float delta)
    {
        if (!CanMove)
            return;
        if (MoveDirection == Vector3.Zero)
            MoveDirection = PathStack.FirstOrDefault() - GlobalTransform.origin;
        if (MoveDirection.Length() > 0.5)
        {
            LookAtDirection(MoveDirection);
            Vector3 velocity = MoveDirection.Normalized();
            float currentSpeed = Speed;

            //apply jump
            if (MoveDirection.y > MinHeightToJump)
            {
                currentSpeed = Godot.Mathf.Clamp(Math.Abs(MoveDirection.y) * 2.3f, 3f, Godot.Mathf.Inf);
                IsJumping = true;
            }

            //fall or move to the edge before failing
            else if (MoveDirection.y < -MinHeightToJump)
            {
                if (Utils.VectorDistanceWithoutY(PathStack.FirstOrDefault(), GlobalTransform.origin) <= 0.2)
                {
                    Gravity += Vector3.Down * delta * GravityStrength;
                    velocity = (PathStack.FirstOrDefault() - GlobalTransform.origin).Normalized() + Gravity;
                }
                else
                    velocity = Utils.VectorRemoveY(MoveDirection).Normalized();
            }

            Vector3 _v = MoveAndSlide(velocity * currentSpeed, Vector3.Up);
            if (GlobalTransform.origin.DistanceTo(PathStack.FirstOrDefault()) >= 0.2)
                return;
        }
        if (PathStack.Count > 0)
            PathStack.RemoveAt(0);
        MoveDirection = Vector3.Zero;
        IsJumping = false;
        Gravity = Vector3.Zero;
        CanMove = PathStack.Count() > 0;
    }

    public void AdjustToCenter()
    {
        MoveDirection = GetTile().GlobalTransform.origin - GlobalTransform.origin;
        MoveAndSlide(MoveDirection * Speed * 4, Vector3.Up);
    }

    public void StartAnimator()
    {
        if (MoveDirection == Vector3.Zero)
            Animator.Travel("IDLE");
        else if (IsJumping)
            Animator.Travel("JUMP");
    }

    public void ApplyMovement(float delta)
    {
        if (PathStack.Count != 0)
            FollowThePath(delta);
        else
            AdjustToCenter();
    }

    public void DoWait()
    {
        CanMove = false;
        CanAttack = false;
    }

    public bool DoAttack(Pawn pawn, float delta)
    {
        LookAtDirection(pawn.GlobalTransform.origin - GlobalTransform.origin);
        if (CanAttack && WaitDelay > MinTimeForAttack / 4)
        {
            pawn.CurrHealth = Godot.Mathf.Clamp(pawn.CurrHealth - AttackPower, 0, 999);
            CanAttack = false;
        }
        if (WaitDelay < MinTimeForAttack)
        {
            WaitDelay += delta;
            return false;
        }
        WaitDelay = 0;
        return true;
    }

    public void Reset()
    {
        CanMove = true;
        CanAttack = true;
    }

    public bool CanAct()
    {
        return (CanMove || CanAttack) && CurrHealth>0;
    }

    private void LoadStats()
    {
        MoveRadius = Utils.GetPawnMoveRadius(PawnClass);
        JumpHeight = Utils.GetPawnJumpHeight(PawnClass);
        AttackRadius = Utils.GetPawnAttackRadius(PawnClass);
        AttackPower = Utils.GetPawnAttackPower(PawnClass);
        MaxHealth = Utils.GetPawnHealth(PawnClass);
        CurrHealth = MaxHealth;
    }

    private void LoadAnimatorSprite()
    {
        Animator = AnimationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
        Animator.Start("IDLE");
        AnimationTree.Active = true;

        Character.Texture = Utils.GetPawnSprite(PawnClass);

        NameLabel.Text = PawnName+", The "+PawnClass.ToString();
    }

    public void TintWhenNotAbleToAct()
    {
        Character.Modulate = !CanAct() 
        ? new Color (0.7f, 0.7f, 0.7f) 
        : new Color(1, 1, 1);
    }

    public void DisplayPawnStats(bool characterStatsVisible)
    {
        CharacterStats.Visible = characterStatsVisible;
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
