using Godot;
using System;

public class Level : Spatial
{
    [Export]
    public int RoundWhenPlayerWins = 5;

    int currentRound = 0;

    object TFrom = null;
    object TTo = null;
    object CurrentT = null;

    PlayerController Player;
    EnemyController Enemy;

    public Level()
    {
        // _Ready();
    }

    public override void _Ready()
    {
        Player = GetNode<PlayerController>("Player");
        Enemy = GetNode<EnemyController>("Enemy");

        Arena arena = GetNode<Arena>("Arena");
        TacticsCamera tacticsCamera = GetNode<TacticsCamera>("TacticsCamera");
        PlayerControllerUI playerControllerUI = GetNode<PlayerControllerUI>("PlayerControllerUI");

        Player.Configure(arena, tacticsCamera, playerControllerUI);
        Enemy.Configure(arena, tacticsCamera);

        // FirstObserverAttachments();
    }


    private void FirstObserverAttachments()
    {
        foreach(APawn pawn in Enemy.AllActiveUnits)
        {
            pawn.Attach(Enemy);
        }
    }


    public void TurnHandler(float delta)
    {
        if (Enemy.CanFirstAct())
            Enemy.FirstAct(delta);
        else if (Player.CanAct())
            Player.Act(delta);
        else if (Enemy.CanSecondAct())
            Enemy.SecondAct(delta);
        else
        {
            currentRound++;
            var print = string.Format("Round {0} finished. New round is: {1}", currentRound-1, currentRound);
            Player.Reset();
            Enemy.Reset();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        TurnHandler(delta);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
