using System.Numerics;
using Godot;
using System;
using System.Collections.Generic;

public class Spawner : Spatial
{

    Godot.Collections.Array<string> possibleNames = new Godot.Collections.Array<string> { "K'", "Maxima", "Ryo", "Robert", "Heidern" };
    Godot.Collections.Array<Spatial> Points;


    public override void _Ready()
    {
        Points = GetChildren().As<Spatial>();
    }

    public Godot.Collections.Array<EnemyPawn> SpawnEnemies()
    {
        Godot.Collections.Array<EnemyPawn> enemyPawns = new Godot.Collections.Array<EnemyPawn>();
        Points.Shuffle();
        int numberToSpawn = 3;
        for (int i = 0; i < numberToSpawn; i++)
        {
            EnemyPawn enemyPawn = GetRandomEnemyPawn();
            enemyPawn.Translation = Points[i].Translation;
            enemyPawns.Add(enemyPawn);
        }
        return enemyPawns;
    }

    private EnemyPawn GetRandomEnemyPawn()
    {
        EnemyPawn enemyPawn = new EnemyPawn();
        enemyPawn.PawnClass = PawnClass.Skeleton;
        enemyPawn.PawnStrategy = PawnStrategy.Flank;
        enemyPawn.PawnName = possibleNames.GetRandom();
        return enemyPawn;
    }
}
