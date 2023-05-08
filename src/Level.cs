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

    LevelInfo LevelInfo;
    private bool nextLevelExists;
    LevelInfo NextLevelInfo;
    PackedScene NextLevel;

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

        LevelManagerOperations();
    }

    private void LevelManagerOperations()
    {
        LevelInfo = LevelManager.GetCurrentLevelInformation();
        nextLevelExists = LevelManager.NextLevelExists();
        if (nextLevelExists)
        {
            NextLevelInfo = LevelManager.GetNextLevel();
            NextLevel = ResourceLoader.Load<PackedScene>(NextLevelInfo.LevelPath);
        }
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
        if (isGameLost)
        {
            GameLost();
        }
        currentRound++;
        if (currentRound == LevelInfo.RoundsToWin)
        {
            LevelWonOperation();
        }
        
        var enemies = Enemy.SpawnEnemies();
        Player.NotifyAboutNewEnemies(enemies);
        var print = string.Format("Round {0} finished. New round is: {1}", currentRound - 1, currentRound);
        GD.Print(print);
        Player.Reset();
        Enemy.Reset();
    }

    private void GameLost()
    {
        GD.Print("TODO game lost. Do a popup window");
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
        if (nextLevelExists)
        {
            LevelManager.CurrentLevel++;
            GD.Print("TODO Popup for next level");
            
        }
        else
        {

        }
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

    private void CreateDefaultConfig()
    {
        var config = new ConfigFile();
        var lvl1RoundsToWin = 5;
        var lvl2RoundsToWin = 4;
        var lvl3RoundsToWin = 6;
        var lvl4RoundsToWin = 7;

        var lvl1AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher };
        var lvl2AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher, (int)PawnClass.SkeletonBomber };
        var lvl3AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher, (int)PawnClass.SkeletonBomber, (int)PawnClass.SkeletonMedic };
        var lvl4AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher, (int)PawnClass.SkeletonBomber, (int)PawnClass.SkeletonMedic, (int)PawnClass.SkeletonHero };



        config.SetValue("level1", "RoundsToWin", lvl1RoundsToWin);
        config.SetValue("level1", "AllowedEnemies", lvl1AllowedEnemies);
        config.SetValue("level2", "RoundsToWin", lvl2RoundsToWin);
        config.SetValue("level2", "AllowedEnemies", lvl2AllowedEnemies);
        config.SetValue("level3", "RoundsToWin", lvl3RoundsToWin);
        config.SetValue("level3", "AllowedEnemies", lvl3AllowedEnemies);
        config.SetValue("level4", "RoundsToWin", lvl4RoundsToWin);
        config.SetValue("level4", "AllowedEnemies", lvl4AllowedEnemies);

        config.Save("config/defaultConfig.cfg");
    }

}