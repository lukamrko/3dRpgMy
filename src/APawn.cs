using System;
using System.Linq;
using System.Threading;
using Godot;
using Godot.Collections;

public abstract class APawn : KinematicBody
{
    protected const float PI = 3.141593f;
    protected const float Speed = 5f; //todo RETURN TO 5
    protected const int AnimationFrames = 1;
    protected const int MinHeightToJump = 1;
    protected const int GravityStrength = 7;
    protected const int MinTimeForAttack = 1;

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
    protected AnimationNodeStateMachinePlayback Animator = null;
    #endregion

    #region Pathfinding
    public Godot.Collections.Array<Vector3> PathStack = new Godot.Collections.Array<Vector3>();
    protected Vector3 MoveDirection = Vector3.Zero;
    protected bool IsJumping = false;
    protected Vector3 Gravity = Vector3.Zero;
    protected float WaitDelay = 0;
    protected int WaitDelayMilliseconds = 1000;

    #endregion

    protected Label HealthLabel;

    #region Node calling
    protected Sprite3D Character;
    protected Spatial CharacterStats;
    protected AnimationTree AnimationTree;
    protected Label NameLabel;
    protected RayCast CurrTiles;
    #endregion



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
            : dir * new Vector3(0, 0, 1);
        float angle = Vector3.Forward.SignedAngleTo(fixedDir.Normalized(), Vector3.Up) + PI;
        Rotation = Vector3.Up * angle;
    }

    public void StartAnimator()
    {
        if (MoveDirection == Vector3.Zero)
            Animator.Travel("IDLE");
        else if (IsJumping)
            Animator.Travel("JUMP");
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
    }

    public void AdjustToCenter()
    {
        MoveDirection = GetTile().GlobalTransform.origin - GlobalTransform.origin;
        MoveAndSlide(MoveDirection * Speed * 4, Vector3.Up);
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

    public bool DoAttack(APawn pawn, float delta)
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

    public void DoAttackOnLocation(Godot.Collections.Array<APawn> allActiveUnits, Vector3 positionOfAttack, float delta)
    {
        if (CanAttack)
        {
            APawn pawnAtLocation = GetPawnAtAttackLocation(allActiveUnits, positionOfAttack);
            if (pawnAtLocation != null)
            {
                pawnAtLocation.CurrHealth = Godot.Mathf.Clamp(pawnAtLocation.CurrHealth - AttackPower, 0, 999);
            }
            CanAttack = false;
        }
        System.Threading.Thread.Sleep(WaitDelayMilliseconds);
    }

    private APawn GetPawnAtAttackLocation(Array<APawn> allActiveUnits, Vector3 positionOfAttack)
    {
        GD.Print(String.Format("Target position {0}", positionOfAttack));
        foreach(APawn pawn in allActiveUnits)
        {
            GD.Print(String.Format("Pawn {0}, position: {1}", pawn.PawnName, pawn.Translation.Rounded()));
            Vector3 directionTowardsPawn = this.Translation.DirectionTo(pawn.Translation).Rounded();
            // if (pawn.Translation.Rounded().Equals(positionOfAttack))
            //     return pawn;
            if (directionTowardsPawn.Equals(positionOfAttack))
                return pawn;
        }
        return null;
    }


    public void Reset()
    {
        CanMove = true;
        CanAttack = true;
    }


    protected void LoadStats()
    {
        MoveRadius = Utils.GetPawnMoveRadius(PawnClass);
        JumpHeight = Utils.GetPawnJumpHeight(PawnClass);
        AttackRadius = Utils.GetPawnAttackRadius(PawnClass);
        AttackPower = Utils.GetPawnAttackPower(PawnClass);
        MaxHealth = Utils.GetPawnHealth(PawnClass);
        CurrHealth = MaxHealth;
    }

    protected void LoadAnimatorSprite()
    {
        Animator = AnimationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
        Animator.Start("IDLE");
        AnimationTree.Active = true;

        Character.Texture = Utils.GetPawnSprite(PawnClass);

        NameLabel.Text = PawnName + ", The " + PawnClass.ToString();
    }

    public void DisplayPawnStats(bool characterStatsVisible)
    {
        CharacterStats.Visible = characterStatsVisible;
    }

    public abstract void TintWhenNotAbleToAct();

}