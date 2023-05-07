using Godot;
using System;
using System.Linq;

public partial class Level : Node3D
{
    [Export]
    public int RoundWhenPlayerWins = 5;
    private const int minNumberOfTotems = 1;
    int currentRound = 0;

    PlayerController Player;
    EnemyController Enemy;
    Spawner Spawner;
    TacticsCamera TacticsCamera;

    public override void _Ready()
    {
        Player = GetNode<PlayerController>("Player");
        Enemy = GetNode<EnemyController>("Enemy");

        Arena arena = GetNode<Arena>("Arena");
        TacticsCamera = GetNode<TacticsCamera>("TacticsCamera");
        PlayerControllerUI playerControllerUI = GetNode<PlayerControllerUI>("PlayerControllerUI");

        Player.Configure(arena, TacticsCamera, playerControllerUI);
        Enemy.Configure(arena, TacticsCamera);

        Spawner = GetNode<Spawner>("EnemySpawner");

        GD.Print("Test physics: " + this.IsPhysicsProcessing());
    }


    private void FirstObserverAttachments()
    {
        foreach (APawn pawn in Enemy.AllActiveUnits)
        {
            pawn.Attach(Enemy);
        }
    }


    public void TurnHandler(double delta)
    {
        if (Enemy.ShouldApplyForce())
        {
            Enemy.DoForcedMovement(delta);
        }
        else if (Player.ShouldApplyForce())
        {
            Player.DoForcedMovement(delta);
        }
        else if (Enemy.CanFirstAct())
        {
            Enemy.FirstAct(delta);
        }
        else if (Player.CanAct())
        {
            Player.Act(delta);
        }
        else if (Enemy.CanSecondAct())
        {
            Enemy.SecondAct(delta);
        }
        else
        {
            TurnOverOperation();
        }
    }

    private void TurnOverOperation()
    {
        var isGameLost = CheckIfGameIsLost();
        currentRound++;
        var enemies = Enemy.SpawnEnemies();
        Player.NotifyAboutNewEnemies(enemies);
        var print = string.Format("Round {0} finished. New round is: {1}", currentRound - 1, currentRound);
        GD.Print(print);
        if (currentRound == RoundWhenPlayerWins)
        {
            LevelWonOperation();
        }
        Player.Reset();
        Enemy.Reset();
    }

    private bool CheckIfGameIsLost()
    {
        var numberOfTotems = Player.PlayerPawns
            .Where(x => x.PawnClass == PawnClass.Totem)
            .Count();
        if (numberOfTotems <= minNumberOfTotems)
        {
            return true;
        }
        var numberOfPLayers = Player.PlayerPawns
            .Where(x => x.PawnClass != PawnClass.Totem)
            .Count();
        if (numberOfPLayers <= 0)
        {
            return true;
        }
        return false;
    }

    private void LevelWonOperation()
    {
        var print = string.Format("Battle was hard, but we won");
        GD.Print(print);
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveCamera();
        CameraRotation();
        TurnHandler(delta);
    }

    #region Camera
    public void MoveCamera()
    {
        float h = -Input.GetActionStrength("camera_left") + Input.GetActionStrength("camera_right");
        float v = Input.GetActionStrength("camera_forward") - Input.GetActionStrength("camera_backwards");
        TacticsCamera.MoveCamera(h, v, false);
    }

    public void CameraRotation()
    {
        if (Input.IsActionJustPressed("camera_rotate_left"))
        {
            TacticsCamera.YRot -= 90;
        }
        if (Input.IsActionJustPressed("camera_rotate_right"))
        {
            TacticsCamera.YRot += 90;
        }
    }
    #endregion 

}
