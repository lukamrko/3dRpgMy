using Godot;
using System;

public static class Utils
{
    private static string KnightSprite = "res://assets/sprites/characters/chr_pawn_knight.png";
    private static string ArcherSprite = "res://assets/sprites/characters/chr_pawn_archer.png";
    private static string ChemistSprite = "res://assets/sprites/characters/chr_pawn_chemist.png";
    private static string ClericSprite = "res://assets/sprites/characters/chr_pawn_mage.png";
    private static string SkeletonCptSprite = "res://assets/sprites/characters/chr_pawn_skeleton_cpt.png";
    private static string SkeletonSprite = "res://assets/sprites/characters/chr_pawn_skeleton.png";
    private static string SkeletonArcherSprite = "res://assets/sprites/characters/chr_pawn_skeleton_archer.png";
    private static string SkeletonMageSprite = "res://assets/sprites/characters/chr_pawn_skeleton_mage.png";


    static string TileSrc = "res://src/Tile.cs";

    /// <summary>
    /// Given a Spatial Node as parameter (tilesObj), this function will iterate over each
    /// of its children converting them into a static body and attaching the Tile.cs script.
    /// e.g.this function will transform the 'Tiles' into the following structure:
    /// 	> Tiles:                                > Tiles:
    /// 		> Tile1                                 > StaticBody (Tile.cs):
    /// 		> Tile2                                     > Tile1
    ///         ...                                         > CollisionShape
    /// 		> TileN       -- TRANSFORM INTO ->      > StaticBody2 (Tile.cs):
    /// 													> Tile2
    /// 													> CollisionShape
    ///                                                 ...
    /// 												> StaticBodyN (Tile.cs):
    /// 													> TileN
    /// 													> CollisionShape
    /// This is very useful for configure walkable tiles as fast as possible
    /// especially if the map used was exported from Blender using the Godot Extension
    /// </summary>
    /// <param name="tilesObj"></param>
    public static void ConvertTilesIntoStaticBodies(Spatial tilesObj)
    {
        var script = ResourceLoader.Load<Reference>(TileSrc);
        var tilesObjects = tilesObj.GetChildren().As<MeshInstance>();
        foreach (MeshInstance tileObj in tilesObjects)
        {
            tileObj.CreateTrimeshCollision();
            StaticBody staticBody = tileObj.GetChild(0) as StaticBody;
            staticBody.Translation = tileObj.Translation;

            tileObj.Translation = Vector3.Zero;
            tileObj.Name = "Tile";
            tileObj.RemoveChild(staticBody);
            tilesObj.RemoveChild(tileObj);

            staticBody.AddChild(tileObj);
            var instanceID = staticBody.GetInstanceId();

            staticBody.SetScript(script);
            Tile staticBodyTile = (Tile)GD.InstanceFromId(instanceID);
            staticBodyTile._Ready();
            staticBodyTile.ConfigureTile();
            staticBodyTile.SetProcess(true);
            tilesObj.AddChild(staticBodyTile);
        }
    }

    public static SpatialMaterial CreateMaterial(Color color, Texture texture = null)
    {
        SpatialMaterial material = new SpatialMaterial();
        material.FlagsTransparent = true;
        material.AlbedoColor = color;
        material.AlbedoTexture = texture;
        return material;
    }

    public static Texture GetPawnSprite(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return ResourceLoader.Load(KnightSprite) as Texture;
            case PawnClass.Archer:
                return ResourceLoader.Load(ArcherSprite) as Texture;
            case PawnClass.Chemist:
                return ResourceLoader.Load(ChemistSprite) as Texture;
            case PawnClass.Cleric:
                return ResourceLoader.Load(ClericSprite) as Texture;
            case PawnClass.Skeleton:
                return ResourceLoader.Load(SkeletonSprite) as Texture;
            case PawnClass.SkeletonArcher:
                return ResourceLoader.Load(SkeletonArcherSprite) as Texture;
            case PawnClass.SkeletonCPT:
                return ResourceLoader.Load(SkeletonCptSprite) as Texture;
            case PawnClass.SkeletonMage:
                return ResourceLoader.Load(SkeletonMageSprite) as Texture;
            default:
                return ResourceLoader.Load(KnightSprite) as Texture;
        }
    }

    public static int GetPawnMoveRadius(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 3;
            case PawnClass.Archer:
                return 5;
            case PawnClass.Chemist:
                return 4;
            case PawnClass.Cleric:
                return 4;
            case PawnClass.Skeleton:
                return 12;
            case PawnClass.SkeletonArcher:
                return 8;
            case PawnClass.SkeletonCPT:
                return 6;
            case PawnClass.SkeletonMage:
                return 7;
            default:
                return 1;
        }
    }

    public static float GetPawnJumpHeight(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 0.5f;
            case PawnClass.Archer:
                return 3f;
            case PawnClass.Chemist:
                return 1f;
            case PawnClass.Cleric:
                return 1f;
            case PawnClass.Skeleton:
                return 20f;
            case PawnClass.SkeletonArcher:
                return 4f;
            case PawnClass.SkeletonCPT:
                return 0.5f;
            case PawnClass.SkeletonMage:
                return 1f;
            default:
                return 1f;
        }
    }

    public static int GetPawnAttackRadius(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 1;
            case PawnClass.Archer:
                return 6;
            case PawnClass.Chemist:
                return 3;
            case PawnClass.Cleric:
                return 3;
            case PawnClass.Skeleton:
                return 1;
            case PawnClass.SkeletonArcher:
                return 4;
            case PawnClass.SkeletonCPT:
                return 1;
            case PawnClass.SkeletonMage:
                return 3;
            default:
                return 1;
        }
    }

    public static int GetPawnAttackPower(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 3;
            case PawnClass.Archer:
                return 2;
            case PawnClass.Chemist:
                return 1;
            case PawnClass.Cleric:
                return 1;
            case PawnClass.Skeleton:
                return 1;
            case PawnClass.SkeletonArcher:
                return 1;
            case PawnClass.SkeletonCPT:
                return 2;
            case PawnClass.SkeletonMage:
                return 3;
            default:
                return 1;
        }
    }

    public static int GetPawnHealth(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 6;
            case PawnClass.Archer:
                return 4;
            case PawnClass.Chemist:
                return 3;
            case PawnClass.Cleric:
                return 3;
            case PawnClass.Skeleton:
                return 2;
            case PawnClass.SkeletonArcher:
                return 1;
            case PawnClass.SkeletonCPT:
                return 4;
            case PawnClass.SkeletonMage:
                return 3;
            default:
                return 1;
        }
    }

    public static Vector3 VectorRemoveY(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public static float VectorDistanceWithoutY(Vector3 a, Vector3 b)
    {
        return VectorRemoveY(a).DistanceTo(VectorRemoveY(b));
    }
}