using System.Collections.Generic;
using System.Linq;
using Godot;
using Vector3 = Godot.Vector3;

public partial class PlayerSpawner : Node3D
{
	readonly Dictionary<int, PlayerPawn> presetForSpawn = new Dictionary<int, PlayerPawn>
	{
		{
			0,
			new PlayerPawn
			{
				PawnClass = PawnClass.Knight,
				PawnName = "Conan",
				PawnStrategy = PawnStrategy.Brute
			}
		},
        {
            1,
            new PlayerPawn
            {
                PawnClass = PawnClass.Archer,
                PawnName = "Red Sonya",
                PawnStrategy = PawnStrategy.Brute

            }
        },
        {
            2,
            new PlayerPawn
            {
                PawnClass = PawnClass.Chemist,
                PawnName = "Alhazred",
                PawnStrategy = PawnStrategy.Brute
            }
        }
	};

    Godot.Collections.Array<StaticBody3D> Points;
    PackedScene PlayerScene = new PackedScene();


    public override void _Ready()
    {
        Points = GetChildren().As<StaticBody3D>();
        PlayerScene = GD.Load<PackedScene>("res://assets/tscn/Pawn.tscn");
    }

    public Dictionary<PlayerPawn, Vector3> InitialPlayerSpawn()
    {
        var playerPawns = new Dictionary<PlayerPawn, Vector3>();
        int numberToSpawn = Points.Count;
        for (int i = 0; i < numberToSpawn; i++)
        {
            var playerPawn = GetPlayerNode(i) as PlayerPawn;
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
            // playerPawn.GlobalPosition = pointTranslation;

            GD.Print("Player pawn name:" + playerPawn.PawnName);
            GD.Print("Player pawn pos:" + pointTranslation);
            playerPawn.Visible = true;
            playerPawns.Add(playerPawn, pointTranslation);
        }

        return playerPawns;
    }

    private Node GetPlayerNode(int index)
    {
		PlayerPawn helperPawn;
        var node = PlayerScene.Instantiate();
        var playerPawn = node as PlayerPawn;
		if(presetForSpawn.TryGetValue(index, out helperPawn))
		{
            playerPawn.PawnClass = helperPawn.PawnClass;
            playerPawn.PawnStrategy = helperPawn.PawnStrategy;
            playerPawn.PawnName = helperPawn.PawnName;
		}
		else
		{
            playerPawn.PawnClass = PawnClass.Totem;
            playerPawn.PawnStrategy = PawnStrategy.Brute;
            playerPawn.PawnName = "Totem";
		}
        return node;
    }




}
