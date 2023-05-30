using System.Linq;
using Godot;
using Vector3 = Godot.Vector3;

public partial class Spawner : Node3D
{

	Godot.Collections.Array<string> possibleNames = new Godot.Collections.Array<string> { "K'", "Maxima", "Ryo", "Robert", "Heidern", "Clark", "Ralf" };
	// Godot.Collections.Array<PawnClass> possibleClasses = new Godot.Collections.Array<PawnClass> 
	// {
	// 	// PawnClass.SkeletonWarrior,
	// 	// PawnClass.SkeletonArcher,
	// 	// PawnClass.SkeletonBomber
	// 	PawnClass.SkeletonMedic
	// };

    Godot.Collections.Array<PawnStrategy> possibleStrategies = new Godot.Collections.Array<PawnStrategy>
    {
        PawnStrategy.Brute,
        PawnStrategy.ObjectiveSniper
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
            enemyPawn.GlobalPosition = pointTranslation;

            GD.Print("Enemy name:" + enemyPawn.PawnName);
            GD.Print("Enemy pos:" + enemyPawn.GlobalPosition);
            enemyPawn.Visible = true;
			enemyPawns.Add(enemyPawn);
		}

		return enemyPawns;
	}

	private Node GetEnemyNode(Godot.Collections.Array<PawnClass> allowedEnemies)
	{
		var node = Scene.Instantiate();
		var enemyPawn = node as EnemyPawn;
		enemyPawn.PawnClass = allowedEnemies.GetRandom();
		enemyPawn.PawnStrategy = possibleStrategies.GetRandom();
		enemyPawn.PawnName = possibleNames.GetRandom();
		return node;
	}

}
