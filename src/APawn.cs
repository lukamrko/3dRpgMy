using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Godot;
using Godot.Collections;

public abstract partial class APawn : CharacterBody3D, ISubject
{
    private const int PushDamage = 1;
    private const int WallHeightToGetDamaged = 1;
    protected const float PI = 3.141593f;
    protected const float Speed = 5f; //todo RETURN TO 5
    protected const int AnimationFrames = 1;
    protected const int MinHeightToJump = 1;
    protected const int GravityStrength = 7;
    protected const int MinTimeForAttack = 1;
    public const int DistanceBetweenTiles = 1;


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
            HealthLabel.Text = value.ToString() + "/" + MaxHealth.ToString();
            HealthLabel._Draw();
            // if (currHealth <= 0)
            // {
            //     GD.Print("I have died, but my time will come!");
            //     this.QueueFree();
            // }
        }
    }

    public AudioStreamPlayer SoundPawnAttack;
    #endregion

    #region Animation
    [Export]
    public int CurrFrame = 0;
    protected AnimationNodeStateMachinePlayback Animator = null;
    private const float dotOnWhichToRotate = 0.306f;
    #endregion

    #region Pathfinding
    public Godot.Collections.Array<Vector3> PathStack = new Godot.Collections.Array<Vector3>();
    public Vector3 MoveDirection = Vector3.Zero;
    protected bool IsJumping = false;
    protected Vector3 Gravity = Vector3.Zero;
    protected float WaitDelay = 0;
    protected int WaitDelayMilliseconds = 1000;

    #endregion

    protected Label HealthLabel;

    #region Node calling
    protected Sprite3D Character;
    protected Node3D CharacterStats;
    protected AnimationTree AnimationTree;
    protected Label NameLabel;
    protected RayCast3D CurrTile;
    #endregion

    protected Area3D deadZone;
    protected Area3D deadZoneDetector;


    internal readonly Godot.Collections.Array<WorldSide> allWorldSides = new Godot.Collections.Array<WorldSide>
    {
        WorldSide.North,
        WorldSide.West,
        WorldSide.South,
        WorldSide.East
    };

    public bool shouldBeForciblyMoved = false;
    public Vector3 directionOfForcedMovement = Vector3.Zero;

    public Tile GetTile()
    {
        var tile = CurrTile.GetCollider() as Tile;
        if (tile is null)
        {
            var isColliding = CurrTile.IsColliding();
            var space = GetWorld3D().DirectSpaceState;
            StackTrace st = new StackTrace();
            GD.Print("Current tiles colliding: " + isColliding);
            GD.Print("Stacktrace: " + st);

        }
        return tile;
    }

    public void RotatePawnSprite()
    {
        Vector3 cameraForward = -GetViewport().GetCamera3D().GlobalTransform.Basis.Z;
        float dot = GlobalTransform.Basis.Z.Dot(cameraForward);
        Character.FlipH = GlobalTransform.Basis.X.Dot(cameraForward) > 0;
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
        Vector3 fixedDir = Math.Abs(dir.X) > Math.Abs(dir.Z)
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


    public void FollowThePath(double delta)
    {
        if (!CanMove)
        {
            return;
        }

        if (MoveDirection == Vector3.Zero)
        {
            MoveDirection = PathStack.FirstOrDefault() - GlobalTransform.Origin;
        }

        //TODO extract 0.5 to const
        if (MoveDirection.Length() > 0.5)
        {
            LookAtDirection(MoveDirection);
            Vector3 velocity = MoveDirection.Normalized();
            float currentSpeed = Speed;

            //apply jump
            if (MoveDirection.Y > MinHeightToJump)
            {
                currentSpeed = Godot.Mathf.Clamp(Math.Abs(MoveDirection.Y) * 2.3f, 3f, Godot.Mathf.Inf);
                IsJumping = true;
            }

            //fall or move to the edge before failing
            else if (MoveDirection.Y < -MinHeightToJump)
            {
                Gravity += Vector3.Down * (float)delta * GravityStrength;
                if (Utils.VectorDistanceWithoutY(PathStack.FirstOrDefault(), GlobalTransform.Origin) <= 0.2)
                {
                    velocity = (PathStack.FirstOrDefault() - GlobalTransform.Origin).Normalized() + Gravity;
                }
                else
                {
                    velocity = Utils.VectorRemoveY(MoveDirection).Normalized() + Gravity;
                }
            }
            //TODO this is most probable MoveAndSlide refactor
            // Vector3 _v = MoveAndSlide(velocity * currentSpeed, Vector3.Up);
            this.Velocity = velocity * currentSpeed; // * Vector3.Up;
            MoveAndSlide();

            if (GlobalTransform.Origin.DistanceTo(PathStack.FirstOrDefault()) >= 0.2)
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
            tilePoint = GlobalTransform.Origin;
            return;
        }
        else
        {
            tilePoint = tile.GlobalTransform.Origin;
        }
        MoveDirection = tilePoint - GlobalTransform.Origin + Gravity;

        this.Velocity = MoveDirection * Speed * 4; //*Vector3.Up;
        MoveAndSlide();
        // MoveAndSlide(MoveDirection * Speed * 4, Vector3.Up);
    }

    public void ApplyMovement(double delta)
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

    private void ForciblyMovePawn(double delta)
    {
        if (MoveDirection == Vector3.Zero)
        {
            MoveDirection = PathStack.FirstOrDefault() - GlobalTransform.Origin;
        }
        if (MoveDirection.Length() > 0.5)
        {
            Vector3 velocity = MoveDirection.Normalized();
            float currentSpeed = Speed;

            //apply jump
            // if (MoveDirection.Y > MinHeightToJump)
            // {
            //     currentSpeed = Godot.Mathf.Clamp(Math.Abs(MoveDirection.Y) * 2.3f, 3f, Godot.Mathf.Inf);
            //     IsJumping = true;
            // }

            // //fall or move to the edge before failing
            // else 
            if (MoveDirection.Y < -MinHeightToJump)
            {
                Gravity += Vector3.Down * (float)delta * GravityStrength;
                if (Utils.VectorDistanceWithoutY(PathStack.FirstOrDefault(), GlobalTransform.Origin) <= 0.2)
                {
                    velocity = (PathStack.FirstOrDefault() - GlobalTransform.Origin).Normalized() + Gravity;
                }
                else
                {
                    velocity = Utils.VectorRemoveY(MoveDirection).Normalized() + Gravity;
                }
            }
            this.Velocity = velocity * currentSpeed; //* Vector3.Up;
            MoveAndSlide();
            if (GlobalTransform.Origin.DistanceTo(PathStack.FirstOrDefault()) >= 0.2)
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

    public void DoAttack(APawn targetPawn, Godot.Collections.Array<APawn> AllActiveUnits)
    {
        LookAtDirection(targetPawn.GlobalTransform.Origin - GlobalTransform.Origin);
        if (CanAttack)
        {
            DoIndirectAttacks(targetPawn, AllActiveUnits);
            DealDirectDamageAndRemoveIfDead(targetPawn, AttackPower);
            CanAttack = false;
        }
        GD.Print("Pretend I do attack!");
    }

    public void DoCharacterActionOnTile(Array<APawn> allActiveUnits, Tile attackableTile)
    {
        LookAtDirection(attackableTile.GlobalTransform.Origin - GlobalTransform.Origin);
        switch (PawnClass)
        {
            case PawnClass.Knight:
            case PawnClass.Archer:
            case PawnClass.Cleric:
                NormalPlayerUnitAttack(allActiveUnits, attackableTile);
                break;
            case PawnClass.Chemist:
                ChemistAttack(allActiveUnits, attackableTile);
                break;
            case PawnClass.SkeletonWarrior:
            case PawnClass.SkeletonArcher:
            case PawnClass.SkeletonHero:
                NormalNPCAttack(allActiveUnits, attackableTile);
                break;
            case PawnClass.SkeletonBomber:
                BomberAttack(allActiveUnits);
                break;
            case PawnClass.SkeletonMedic:
                MedicAction(allActiveUnits);
                break;
            default:
                NormalPlayerUnitAttack(allActiveUnits, attackableTile);
                break;
        }
        this.SoundPawnAttack.Play();
        this.CanAttack = false;
    }

    private void MedicAction(Array<APawn> allActiveUnits)
    {
        foreach (var pawn in allActiveUnits)
        {
            if (pawn is EnemyPawn allyPawn)
            {
                if (allyPawn.CurrHealth < allyPawn.MaxHealth)
                {
                    allyPawn.CurrHealth++;
                    GD.Print("Healing poor bones: " + allyPawn.PawnName);
                }
            }
        }
    }

    private void BomberAttack(Array<APawn> allActiveUnits)
    {
        DealDirectDamageAndRemoveIfDead(this, 999);
    }

    private void NormalNPCAttack(Array<APawn> allActiveUnits, Tile attackableTile)
    {
        var additionalDamage = GetSkeletonDmgBuffs();
        var targetPawn = attackableTile.GetObjectAbove() as APawn;
        if (targetPawn is object && CanAttack)
        {
            DealDirectDamageAndRemoveIfDead(targetPawn, AttackPower);
        }
        CanAttack = false;
        GD.Print("Pretend I do attack!");
    }

    private int GetSkeletonDmgBuffs()
    {
        var additionalDamage = 0;
        foreach (var worldSide in allWorldSides)
        {
            var worldSideTile = GetTile().GetNeighborAtWorldSide(worldSide);
            if (worldSideTile is null)
            {
                continue;
            }
            if (worldSideTile.GetObjectAbove() is EnemyPawn skeletonAlly)
            {
                if (skeletonAlly.PawnClass == PawnClass.SkeletonHero)
                {
                    additionalDamage++;
                }
            }
        }
        return additionalDamage;
    }

    private void NormalPlayerUnitAttack(Array<APawn> allActiveUnits, Tile attackableTile)
    {
        var targetPawn = attackableTile.GetObjectAbove() as APawn;
        if (targetPawn is object && CanAttack)
        {
            DoIndirectAttacks(targetPawn, allActiveUnits);
            DealDirectDamageAndRemoveIfDead(targetPawn, AttackPower);
        }
        CanAttack = false;
        GD.Print("Pretend I do attack!");
    }

    private void ChemistAttack(Array<APawn> allActiveUnits, Tile attackableTile)
    {
        if (CanAttack)
        {
            DoIndirectCrossAttack(attackableTile);

            if (attackableTile.GetObjectAbove() is APawn targetPawn)
            {
                DealDirectDamageAndRemoveIfDead(targetPawn, AttackPower);
            }
        }
        GD.Print("I have become death, destroyer of skeletons!");
    }

    private void DoIndirectCrossAttack(Tile attackableTile, int damage = 0)
    {
        foreach (var worldSide in allWorldSides)
        {
            var worldSideTile = attackableTile.GetNeighborAtWorldSide(worldSide);
            if (worldSideTile is null)
            {
                continue;
            }
            if (worldSideTile.GetObjectAbove() is APawn worldSidePawn)
            {
                switch (PawnClass)
                {
                    case PawnClass.SkeletonBomber:
                        DealDirectDamageAndRemoveIfDead(worldSidePawn, damage);
                        break;
                    case PawnClass.Chemist:
                        DoTheRepeatingCongaLineAttack(worldSidePawn, worldSide);
                        break;
                }
            }
        }
    }

    private void BomberDeathAftermath(APawn bomberPawn)
    {
        var explosionTile = bomberPawn.GetTile();
        DoIndirectCrossAttack(explosionTile, damage: 999);
    }

    private void DoIndirectAttacks(APawn targetPawn, Array<APawn> allActiveUnits)
    {
        var directionTowardsPawn = targetPawn.Position.Rounded() - this.Position.Rounded();
        var distanceBetweenBehindAndTowardDirection = new Vector3
        {
            X = directionTowardsPawn.X != 0
                ? DistanceBetweenTiles * Math.Sign(directionTowardsPawn.X)
                : 0,
            Z = directionTowardsPawn.Z != 0
                ? DistanceBetweenTiles * Math.Sign(directionTowardsPawn.Z)
                : 0,
            Y = 0
        };

        var sideWherePawnIsGettingPushed = GetSideOfWorldBasedOnVector(distanceBetweenBehindAndTowardDirection);
        var targetPawnTile = targetPawn.GetTile();
        var tileWherePawnIsGettingPushed = targetPawnTile.GetNeighborAtWorldSide(sideWherePawnIsGettingPushed);
        DoTheRepeatingCongaLineAttack(targetPawn, sideWherePawnIsGettingPushed);
    }

    private void DoTheRepeatingCongaLineAttack(APawn targetPawn, WorldSide sideWherePawnIsGettingPushed)
    {
        if (targetPawn.PawnClass == PawnClass.Totem)
        {
            // DealDirectDamageAndRemoveIfDead(targetPawn, PushDamage);
            // return;
        }
        var targetPawnTile = targetPawn.GetTile();
        var tileWherePawnIsGettingPushed = targetPawnTile.GetNeighborAtWorldSide(sideWherePawnIsGettingPushed);

        // this should be out of the map state
        if (tileWherePawnIsGettingPushed is null)
        {
            var positionOfOutOfBounds = targetPawnTile.GetNeighborPositionAtWorldSide(sideWherePawnIsGettingPushed);
            var forcedFallDirection = (positionOfOutOfBounds - targetPawn.Position).Rounded();

            targetPawn.shouldBeForciblyMoved = true;
            targetPawn.directionOfForcedMovement = forcedFallDirection;
            return;
        }

        var forcedMovementDirection = (tileWherePawnIsGettingPushed.Position - targetPawnTile.Position).Rounded();

        if (Math.Abs(tileWherePawnIsGettingPushed.Position.Y - targetPawnTile.Position.Y) >= WallHeightToGetDamaged)
        {
            DealDirectDamageAndRemoveIfDead(targetPawn, PushDamage);
            GD.Print("The wall is too big!");
            return;
        }

        var potentialPawn = tileWherePawnIsGettingPushed.GetObjectAbove() as APawn;

        if (potentialPawn is null)
        {
            GD.Print("Nobody behind pawn boss!");
            targetPawn.shouldBeForciblyMoved = true;
            targetPawn.directionOfForcedMovement = forcedMovementDirection;
        }
        else
        {
            DoTheRepeatingCongaLineAttack(potentialPawn, sideWherePawnIsGettingPushed);
            DealDirectDamageAndRemoveIfDead(potentialPawn, PushDamage);
            DealDirectDamageAndRemoveIfDead(targetPawn, PushDamage);
            GD.Print("BOSS WE GOT EM! There is somebody behind him");
        }
    }

    private Tile GetNeighboringTile(Tile targetPawnTile, WorldSide sideWherePawnIsGettingPushed)
    {

        return null;
    }

    public WorldSide GetSideOfWorldBasedOnVector(Vector3 distanceBetweenBehindAndTowardDirection)
    {
        var x = distanceBetweenBehindAndTowardDirection.X;
        if (x < 0)
        {
            return WorldSide.West;
        }
        else if (x > 0)
        {
            return WorldSide.East;
        }

        var z = distanceBetweenBehindAndTowardDirection.Z;
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

    public void DealDirectDamageAndRemoveIfDead(APawn pawn, int damage)
    {
        pawn.CurrHealth -= damage;
        if (pawn.PawnClass == PawnClass.SkeletonBomber)
        {
            BomberDeathAftermath(pawn);
        }
        if (pawn.CurrHealth <= 0)
        {
            pawn.Notify();
            pawn.QueueFree();
        }
    }



    //TODO This method is similar to DoAttackOnTile. Might merge them
    /// <summary>
    /// Attack used by enemy pawns for now
    /// </summary>
    /// <param name="allActiveUnits">All active units on terrain</param>
    /// <param name="positionOfAttack">Vector3 position of attack</param>
    public void DoAttackOnLocation(Godot.Collections.Array<APawn> allActiveUnits, Vector3 positionOfAttack)
    {
        LookAtDirection(positionOfAttack);
        if (CanAttack)
        {
            APawn pawnAtLocation = GetPawnAtAttackLocation(allActiveUnits, positionOfAttack);
            if (pawnAtLocation is object)
            {
                DealDirectDamageAndRemoveIfDead(pawnAtLocation, AttackPower);
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
            GD.Print(String.Format("Pawn {0}, position: {1}", pawn.PawnName, pawn.Position.Rounded()));
            Vector3 directionTowardsPawn = pawn.Position.Rounded();
            //Y gets weird with setup and I would need to ignore it
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
        var audioStream = Utils.GetPawnAttackSound(PawnClass);
        SoundPawnAttack.Stream = audioStream;
        SoundPawnAttack.VolumeDb = -15f;
        CurrHealth = MaxHealth;
        HealthLabel.Text = CurrHealth.ToString() + "/" + MaxHealth.ToString();
    }

    public void LoadAnimatorSprite()
    {
        Animator = AnimationTree.Get(@"parameters/playback").Obj as AnimationNodeStateMachinePlayback;
        Animator.Start("IDLE");
        AnimationTree.Active = true;
        Character.Texture = Utils.GetPawnSprite(PawnClass);
        if (PawnClass == PawnClass.Totem)
        {
            Character.Offset = new Vector2(0, 30);
        }
        NameLabel.Text = PawnName;
    }

    public void DisplayPawnStats(bool characterStatsVisible)
    {
        CharacterStats.Visible = true;
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
