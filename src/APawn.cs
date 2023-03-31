using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using Godot.Collections;

public abstract class APawn : KinematicBody, ISubject
{
    private const int PushDamage = 1;
    private const int WallHeightToGetDamaged = 1;
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
    private int currHealth = 100;
    public int CurrHealth
    {
        get { return currHealth; }
        set
        {
            currHealth = value;
            if (currHealth <= 0)
            {
                GD.Print("I have died, but my time will come!");
                this.QueueFree();
            }
        }
    }
    #endregion

    #region Animation
    [Export]
    public int CurrFrame = 0;
    protected AnimationNodeStateMachinePlayback Animator = null;
    private const float dotOnWhichToRotate = 0.306f;
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

    private const int distanceBetweenTiles = 1;

    public bool shouldBeForciblyMoved = false;
    public Vector3 directionOfForcedMovement = Vector3.Zero;

    public Tile GetTile()
    {
        var tile = CurrTiles.GetCollider() as Tile;
        if (tile is null)
        {
            var isColliding = CurrTiles.IsColliding();
            GD.Print("Current tiles colliding: " + isColliding);
        }
        return tile;
    }

    public void RotatePawnSprite()
    {
        Vector3 cameraForward = -GetViewport().GetCamera().GlobalTransform.basis.z;
        float dot = GlobalTransform.basis.z.Dot(cameraForward);
        Character.FlipH = GlobalTransform.basis.x.Dot(cameraForward) > 0;
        if (dot < -dotOnWhichToRotate)
        {
            Character.Frame = CurrFrame;
        }
        else if (dot > dotOnWhichToRotate)
        {
            Character.Frame = CurrFrame + 1 * AnimationFrames;
        }
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
        {
            Animator.Travel("IDLE");
        }
        else if (IsJumping)
        {
            Animator.Travel("JUMP");
        }
    }


    public void FollowThePath(float delta)
    {
        if (!CanMove)
        {
            return;
        }

        if (MoveDirection == Vector3.Zero)
        {
            MoveDirection = PathStack.FirstOrDefault() - GlobalTransform.origin;
        }

        //TODO extract 0.5 to const
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
                {
                    velocity = Utils.VectorRemoveY(MoveDirection).Normalized();
                }
            }

            Vector3 _v = MoveAndSlide(velocity * currentSpeed, Vector3.Up);
            if (GlobalTransform.origin.DistanceTo(PathStack.FirstOrDefault()) >= 0.2)
            {
                return;
            }
        }

        if (PathStack.Count > 0)
        {
            PathStack.RemoveAt(0);
        }

        MoveDirection = Vector3.Zero;
        IsJumping = false;
        Gravity = Vector3.Zero;
    }

    public void AdjustToCenter()
    {
        var tile = GetTile();
        var tilePoint = Vector3.Zero;
        //THIS IS WRONG. REFACTOR THIS. TODO
        if (tile is null)
        {
            tile = GetTile();
            tilePoint = GlobalTransform.origin;
            return;
        }
        else
        {
            tilePoint = tile.GlobalTransform.origin;
        }
        MoveDirection = tilePoint - GlobalTransform.origin;
        MoveAndSlide(MoveDirection * Speed * 4, Vector3.Up);
    }

    public void ApplyMovement(float delta)
    {
        if (PathStack.Count != 0)
        {
            if (shouldBeForciblyMoved == true)
            {
                ForciblyMovePawn(delta);
            }
            else
            {
                FollowThePath(delta);
            }
        }
        else
        {
            AdjustToCenter();
        }
    }

    private void ForciblyMovePawn(float delta)
    {
        if (MoveDirection == Vector3.Zero)
        {
            MoveDirection = PathStack.FirstOrDefault() - GlobalTransform.origin;
        }
        if (MoveDirection.Length() > 0.5)
        {
            Vector3 velocity = MoveDirection.Normalized();
            float currentSpeed = Speed;

            //apply jump
            // if (MoveDirection.y > MinHeightToJump)
            // {
            //     currentSpeed = Godot.Mathf.Clamp(Math.Abs(MoveDirection.y) * 2.3f, 3f, Godot.Mathf.Inf);
            //     IsJumping = true;
            // }

            // //fall or move to the edge before failing
            // else 
            if (MoveDirection.y < -MinHeightToJump)
            {
                if (Utils.VectorDistanceWithoutY(PathStack.FirstOrDefault(), GlobalTransform.origin) <= 0.2)
                {
                    Gravity += Vector3.Down * delta * GravityStrength;
                    velocity = (PathStack.FirstOrDefault() - GlobalTransform.origin).Normalized() + Gravity;
                }
                else
                {
                    velocity = Utils.VectorRemoveY(MoveDirection).Normalized();
                }
            }

            Vector3 _v = MoveAndSlide(velocity * currentSpeed, Vector3.Up);
            if (GlobalTransform.origin.DistanceTo(PathStack.FirstOrDefault()) >= 0.2)
            {
                return;
            }
        }

        if (PathStack.Count > 0)
        {
            PathStack.RemoveAt(0);
        }

        MoveDirection = Vector3.Zero;
        IsJumping = false;
        Gravity = Vector3.Zero;
    }

    public void DoWait()
    {
        CanMove = false;
        CanAttack = false;
    }

    public void DoAttack(APawn targetPawn, Godot.Collections.Array<APawn> AllActiveUnits, float delta)
    {
        LookAtDirection(targetPawn.GlobalTransform.origin - GlobalTransform.origin);
        if (CanAttack)
        {
            AnybodyBehindTarget(targetPawn, AllActiveUnits);
            DealDamageAndRemoveIfDead(targetPawn, AttackPower);
            CanAttack = false;
        }
        GD.Print("Pretend I do attack!");
    }

    private void AnybodyBehindTarget(APawn targetPawn, Array<APawn> allActiveUnits)
    {
        var directionTowardsPawn = targetPawn.Translation.Rounded() - this.Translation.Rounded();
        var distanceBetweenBehindAndTowardDirection = new Vector3
        {
            x = directionTowardsPawn.x != 0
                ? distanceBetweenTiles * Math.Sign(directionTowardsPawn.x)
                : 0,
            z = directionTowardsPawn.z != 0
                ? distanceBetweenTiles * Math.Sign(directionTowardsPawn.z)
                : 0,
            y = 0
        };
        var locationBehindDirection = directionTowardsPawn + distanceBetweenBehindAndTowardDirection;

        var sideWherePawnIsGettingPushed = GetSideOfWorldBasedOnVector(distanceBetweenBehindAndTowardDirection);
        var targetPawnTile = targetPawn.GetTile();
        var tileWherePawnIsGettingPushed = targetPawnTile.GetNeighborAtWorldSide(sideWherePawnIsGettingPushed);
        if (tileWherePawnIsGettingPushed.Translation.y - this.Translation.y >= WallHeightToGetDamaged)
        {
            DealDamageAndRemoveIfDead(targetPawn, PushDamage);
            GD.Print("The wall is too big!");
            return;
        }

        var potentialPawn = tileWherePawnIsGettingPushed.GetObjectAbove() as APawn;

        // var neighboringTile = GetNeighboringTile(targetPawnTile, sideWherePawnIsGettingPushed);
        // APawn pawnAtLocation = GetPawnAtAttackLocation(allActiveUnits, locationBehindDirection);
        if (potentialPawn is null)
        {
            //TODO should probably check if is out of bounds or similar stuff
            // Vector3 supposedLocation = (targetPawn.Translation + distanceBetweenBehindAndTowardDirection);
            // Vector3 supposedLocationRounded =supposedLocation.Rounded(); 
            // targetPawn.TranslateObjectLocal(supposedLocationRounded);
            var actualPlaceWherePawnShouldBeMoved = tileWherePawnIsGettingPushed.Translation - targetPawnTile.Translation;
            var actualPlaceWherePawnShouldBeMovedRounded = actualPlaceWherePawnShouldBeMoved.Round();
            GD.Print("Nobody behind pawn boss!");
            targetPawn.shouldBeForciblyMoved = true;
            // targetPawn.directionOfForcedMovement = distanceBetweenBehindAndTowardDirection;
            targetPawn.directionOfForcedMovement = actualPlaceWherePawnShouldBeMovedRounded;

            // var tile = Arena.GetTileAtLocation(supposedLocation);
            // targetPawn.PathStack = Arena.GeneratePathStack(tile);

            //TODO this line pulls enemy towards you. Might be fun
            // targetPawn.TranslateObjectLocal(tileBehindDirection);
        }
        else
        {
            DealDamageAndRemoveIfDead(potentialPawn, PushDamage);
            DealDamageAndRemoveIfDead(targetPawn, PushDamage);
            GD.Print("BOSS WE GOT EM! There is somebody behind him");
        }
    }

    private Tile GetNeighboringTile(Tile targetPawnTile, WorldSide sideWherePawnIsGettingPushed)
    {

        return null;
    }

    private WorldSide GetSideOfWorldBasedOnVector(Vector3 distanceBetweenBehindAndTowardDirection)
    {
        var x = distanceBetweenBehindAndTowardDirection.x;
        if (x < 0)
        {
            return WorldSide.West;
        }
        else if (x > 0)
        {
            return WorldSide.East;
        }

        var z = distanceBetweenBehindAndTowardDirection.z;
        if (z < 0)
        {
            return WorldSide.North;
        }
        else if (z > 0)
        {
            return WorldSide.South;
        }

        return WorldSide.North;
    }

    private void DealDamageAndRemoveIfDead(APawn pawn, int damage)
    {
        pawn.CurrHealth -= damage;
        if (pawn.CurrHealth <= 0)
        {
            pawn.Notify();
            pawn.QueueFree();
        }
    }

    public void DoAttackOnLocation(Godot.Collections.Array<APawn> allActiveUnits, Vector3 positionOfAttack, float delta)
    {
        LookAtDirection(positionOfAttack);
        if (CanAttack)
        {
            APawn pawnAtLocation = GetPawnAtAttackLocation(allActiveUnits, positionOfAttack);
            if (pawnAtLocation is object)
            {
                DealDamageAndRemoveIfDead(pawnAtLocation, AttackPower);
            }
            CanAttack = false;
        }
        GD.Print("Pretend I sleep!");
    }

    private APawn GetPawnAtAttackLocation(Array<APawn> allActiveUnits, Vector3 positionOfAttack)
    {
        GD.Print(String.Format("Target position {0}", positionOfAttack));
        foreach (APawn pawn in allActiveUnits)
        {
            GD.Print(String.Format("Pawn {0}, position: {1}", pawn.PawnName, pawn.Translation.Rounded()));
            Vector3 directionTowardsPawn = pawn.Translation.Rounded();
            if (directionTowardsPawn.Equals(positionOfAttack))
            {
                return pawn;
            }
        }
        return null;
    }


    public void Reset()
    {
        CanMove = true;
        CanAttack = true;
    }


    public void LoadStats()
    {
        MoveRadius = Utils.GetPawnMoveRadius(PawnClass);
        JumpHeight = Utils.GetPawnJumpHeight(PawnClass);
        AttackRadius = Utils.GetPawnAttackRadius(PawnClass);
        AttackPower = Utils.GetPawnAttackPower(PawnClass);
        MaxHealth = Utils.GetPawnHealth(PawnClass);
        CurrHealth = MaxHealth;
    }

    public void LoadAnimatorSprite()
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

    private List<IObserver> _observers = new List<IObserver>();
    public void Attach(IObserver observer)
    {
        this._observers.Add(observer);
    }

    public void Detach(IObserver observer)
    {
        this._observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (var observer in _observers)
        {
            observer.Update(this);
        }
    }
}