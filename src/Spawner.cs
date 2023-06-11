using System.Collections.Generic;
using System.Linq;
using Godot;
using Vector3 = Godot.Vector3;

public partial class Spawner : Node3D
{

	Godot.Collections.Array<string> possibleNames = new Godot.Collections.Array<string> { "K'", "Maxima", "Ryo", "Robert", "Heidern", "Clark", "Ralf", "Geese", "Terry", "Joe", "Andy", "Rugal", "Ash" };

    Godot.Collections.Array<PawnStrategy> possibleStrategies = new Godot.Collections.Array<PawnStrategy>
    {
        PawnStrategy.Brute,
        PawnStrategy.ObjectiveSniper
    };

	Godot.Collections.Array<StaticBody3D> Points;
	PackedScene EnemyScene = new PackedScene();

    public override void _Ready()
	{
		Points = GetChildren().As<StaticBody3D>();
		EnemyScene = GD.Load<PackedScene>("res://assets/tscn/EnemyPawn.tscn");
	}


	public Dictionary<EnemyPawn, Vector3> SpawnEnemies()
	{
        var enemyPawns = new Dictionary<EnemyPawn, Vector3>();
		var allowedEnemies = LevelManager.GetCurrentLevelInformation().AllowedEnemies;
		Points.Shuffle();
		int numberToSpawn = Points.Count;
		for (int i = 0; i < numberToSpawn; i++)
		{
			EnemyPawn enemyPawn = GetEnemyNode(allowedEnemies) as EnemyPawn;
        	var pointTranslation = new Godot.Vector3
			(
               Points[i].GlobalPosition.X,
               Points[i].GlobalPosition.Y,
               Points[i].GlobalPosition.Z
			);
			
            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(pointTranslation + Vector3.Up * 100, pointTranslation);
            var result = spaceState.IntersectRay(query);
			var tilePosY = result["position"].AsVector3().Y;
            pointTranslation.Y = tilePosY;
            // enemyPawn.GlobalPosition = pointTranslation;

            GD.Print("Enemy name:" + enemyPawn.PawnName);
            GD.Print("Enemy pos:" + pointTranslation);
            enemyPawn.Visible = true;
			enemyPawns.Add(enemyPawn, pointTranslation);
		}

		return enemyPawns;
	}

	private Node GetEnemyNode(Godot.Collections.Array<PawnClass> allowedEnemies)
	{
		var node = EnemyScene.Instantiate();
		var enemyPawn = node as EnemyPawn;
		enemyPawn.PawnClass = allowedEnemies.GetRandom();
		enemyPawn.PawnStrategy = PawnStrategy.Brute;
		// enemyPawn.PawnStrategy = possibleStrategies.GetRandom();
		enemyPawn.PawnName = possibleNames.GetRandom();
		return node;
	}

}
