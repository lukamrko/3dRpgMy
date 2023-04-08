using System.Numerics;
using Godot;
using System;
using System.Collections.Generic;

public partial class Spawner : Node3D
{

	Godot.Collections.Array<string> possibleNames = new Godot.Collections.Array<string> { "K'", "Maxima", "Ryo", "Robert", "Heidern" };
	Godot.Collections.Array<PawnClass> possibleClasses = new Godot.Collections.Array<PawnClass> 
	{
		PawnClass.Skeleton3D,
		//PawnClass.SkeletonArcher
	};
	Godot.Collections.Array<StaticBody3D> Points;
	PackedScene Scene = new PackedScene();

	public override void _Ready()
	{
		Points = GetChildren().As<StaticBody3D>();
		Scene = GD.Load<PackedScene>("res://assets/tscn/EnemyPawn.tscn");
	}

	public Godot.Collections.Array<EnemyPawn> SpawnEnemies()
	{
		Godot.Collections.Array<EnemyPawn> enemyPawns = new Godot.Collections.Array<EnemyPawn>();
		Points.Shuffle();
		int numberToSpawn = 4;
		for (int i = 0; i < numberToSpawn; i++)
		{
			EnemyPawn enemyPawn = GetEnemyNode() as EnemyPawn;
			enemyPawn.GlobalPosition = Points[i].GlobalPosition;
			enemyPawn.Visible = true;
			enemyPawns.Add(enemyPawn);
		}
		return enemyPawns;
	}

	private Node GetEnemyNode()
	{
		// var node = Scene.Instance();
		var node = Scene.Instantiate();
		var enemyPawn = node as EnemyPawn;
		enemyPawn.PawnClass = possibleClasses.GetRandom();
		enemyPawn.PawnStrategy = PawnStrategy.Flank;
		enemyPawn.PawnName = possibleNames.GetRandom();
		return node;
	}

}
