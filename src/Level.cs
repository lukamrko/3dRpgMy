using System.Threading;
using Godot;
using System;
using System.Linq;

public partial class Level : Node3D
{
    private const int minNumberOfTotems = 1;
    int currentRound = 1;

    PlayerController PlayerController;
    EnemyController EnemyController;

    TacticsCamera TacticsCamera;

    LevelInfo LevelInfo;
    private bool nextLevelExists;
    LevelInfo NextLevelInfo;
    PackedScene NextLevel;
    EndInfoWindow EndInfoWindow;

    Label valueVictoryRound;
    Label valueCurrentRound;

    public override void _Ready()
    {
        PlayerController = GetNode<PlayerController>("Player");
        EnemyController = GetNode<EnemyController>("Enemy");

        var arena = GetNode<Arena>("Arena");
        TacticsCamera = GetNode<TacticsCamera>("TacticsCamera");
        var playerControllerUI = GetNode<PlayerControllerUI>("PlayerControllerUI");

        EndInfoWindow = GetNode<EndInfoWindow>("EndInfoWindow");
        EndInfoWindow.Visible = false;

        LevelManagerInitialOperations();
        InitialSpawnPawns();

        PlayerController.Configure(arena, TacticsCamera, playerControllerUI);
        EnemyController.Configure(arena, TacticsCamera);

        var levelUI = GetNode("LevelUI");
        valueVictoryRound = levelUI.GetNode<Label>("ColorRectangle/valueVictoryRound");
        valueCurrentRound = levelUI.GetNode<Label>("ColorRectangle/valueCurrentRound");
        valueVictoryRound.Text = LevelInfo.RoundsToWin.ToString();
        valueCurrentRound.Text = currentRound.ToString();

        Thread.Sleep(500);
    }

    private void InitialSpawnPawns()
    {
        var enemyPawns = EnemyController.SpawnEnemies();
        PlayerController.NotifyAboutNewEnemies(enemyPawns);

        var playerPawns = PlayerController.InitialSpawn();
        EnemyController.NotifyAboutSpawnedPlayers(playerPawns);

        PlayerController.Reset();
        EnemyController.Reset();
    }

    private void LevelManagerInitialOperations()
    {
        LevelInfo = LevelManager.GetCurrentLevelInformation();
        nextLevelExists = LevelManager.NextLevelExists();
        if (nextLevelExists)
        {
            NextLevelInfo = LevelManager.GetNextLevelInfo();
            NextLevel = ResourceLoader.Load<PackedScene>(NextLevelInfo.LevelPath);
        }
    }


    public void TurnHandler(double delta)
    {
        if (EnemyController.ShouldApplyForce())
        {
            EnemyController.DoForcedMovement(delta);
        }
        else if (PlayerController.ShouldApplyForce())
        {
            PlayerController.DoForcedMovement(delta);
        }
        else if (EnemyController.CanFirstAct())
        {
            EnemyController.FirstAct(delta);
        }
        else if (PlayerController.CanAct())
        {
            PlayerController.Act(delta);
        }
        else if (EnemyController.CanSecondAct())
        {
            EnemyController.SecondAct(delta);
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
        if (currentRound == LevelInfo.RoundsToWin)
        {
            LevelWonOperation();
        }
        currentRound++;
        valueCurrentRound.Text = currentRound.ToString();
        var enemies = EnemyController.SpawnEnemies();
        PlayerController.NotifyAboutNewEnemies(enemies);
        var print = string.Format("Round {0} finished. New round is: {1}", currentRound - 1, currentRound);
        GD.Print(print);
        PlayerController.Reset();
        EnemyController.Reset();
    }

    private void GameLost()
    {
        EndInfoWindow.SetGameLostUI();
    }

    private bool CheckIfGameIsLost()
    {
        var numberOfTotems = PlayerController.PlayerPawns
            .Where(x => x.PawnClass == PawnClass.Totem)
            .Count();
        if (numberOfTotems <= minNumberOfTotems)
        {
            return true;
        }
        var numberOfPLayers = PlayerController.PlayerPawns
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
            EndInfoWindow.SetLevelWon();
        }
        else
        {
            EndInfoWindow.SetGameWon();
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

}