using System.Numerics;
using Godot;
using System;
using System.Collections.Generic;

public class Spawner : Spatial
{

    Godot.Collections.Array<string> possibleNames = new Godot.Collections.Array<string> { "K'", "Maxima", "Ryo", "Robert", "Heidern" };
    Godot.Collections.Array<StaticBody> Points;


    public override void _Ready()
    {
        Points = GetChildren().As<StaticBody>();
    }

    public Godot.Collections.Array<EnemyPawn> SpawnEnemies()
    {
        Godot.Collections.Array<EnemyPawn> enemyPawns = new Godot.Collections.Array<EnemyPawn>();
        Points.Shuffle();
        int numberToSpawn = 3;
        for (int i = 0; i < numberToSpawn; i++)
        {
            // EnemyPawn enemyPawn = GetRandomEnemyPawn();
            EnemyPawn enemyPawn = GetEnemyNode() as EnemyPawn;
            enemyPawn.GlobalTranslation = Points[i].GlobalTranslation;
            enemyPawn.Visible = true;
            // enemyPawn.Translation = Points[i].Translation;

            enemyPawns.Add(enemyPawn);
        }
        return enemyPawns;
    }

    private Node GetEnemyNode()
    {
        var scene = GD.Load<PackedScene>("res://assets/tscn/EnemyPawn.tscn");
        var node = scene.Instance();
        var enemyPawn = node as EnemyPawn;
        enemyPawn.PawnClass = PawnClass.Skeleton;
        enemyPawn.PawnStrategy = PawnStrategy.Flank;
        enemyPawn.PawnName = possibleNames.GetRandom();
        // AddChild(node);
        return node;
    }

    private EnemyPawn GetRandomEnemyPawn()
    {

        EnemyPawn enemyPawn = new EnemyPawn();
        // var something = (EnemyPawn)enemyPawn.EnemyScene.Instance();
        // something.GLo
        
        enemyPawn.PawnClass = PawnClass.Skeleton;
        enemyPawn.PawnStrategy = PawnStrategy.Flank;
        enemyPawn.PawnName = possibleNames.GetRandom();
        return enemyPawn;
    }
}
