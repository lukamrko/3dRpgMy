using Godot;
using System;

public static class Utils
{
    private static string KNIGHT_SPRITE = "res://assets/sprites/characters/chr_pawn_knight.png";
    private static string ARCHER_SPRITE = "res://assets/sprites/characters/chr_pawn_archer.png";
    private static string CHEMIST_SPRITE = "res://assets/sprites/characters/chr_pawn_chemist.png";
    private static string CLERIC_SPRITE = "res://assets/sprites/characters/chr_pawn_mage.png";
    private static string SKELETON_CPT_SPRITE = "res://assets/sprites/characters/chr_pawn_skeleton_cpt.png";
    private static string SKELETON_SPRITE = "res://assets/sprites/characters/chr_pawn_skeleton.png";
    private static string SKELETON_MAGE_SPRITE = "res://assets/sprites/characters/chr_pawn_skeleton_mage.png";


    static string TileSrc = "res://src/Tile.cs";
    public static void ConvertTilesIntoStaticBodies(Spatial tilesObj)
    {
        var script = ResourceLoader.Load<Reference>(TileSrc);
        foreach (MeshInstance tileObj in tilesObj.GetChildren().As<MeshInstance>())
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
            // Resource script = GD.Load(TileSrc);

            staticBody.SetScript(script);
            // var a = ResourceLoader.Load<Reference>(TileSrc);
            // staticBody.SetScript(ResourceLoader.Load<Reference>(TileSrc));
            Tile staticBodyTile = (Tile)GD.InstanceFromId(instanceID);
            // Tile staticBodyTile = staticBody as Tile;
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
                return ResourceLoader.Load(KNIGHT_SPRITE) as Texture;
            case PawnClass.Archer:
                return ResourceLoader.Load(ARCHER_SPRITE) as Texture;
            case PawnClass.Chemist:
                return ResourceLoader.Load(CHEMIST_SPRITE) as Texture;
            case PawnClass.Cleric:
                return ResourceLoader.Load(CLERIC_SPRITE) as Texture;
            case PawnClass.Skeleton:
                return ResourceLoader.Load(SKELETON_SPRITE) as Texture;
            case PawnClass.SkeletonCPT:
                return ResourceLoader.Load(SKELETON_CPT_SPRITE) as Texture;
            case PawnClass.SkeletonMage:
                return ResourceLoader.Load(SKELETON_MAGE_SPRITE) as Texture;
            default:
                return ResourceLoader.Load(KNIGHT_SPRITE) as Texture;
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
                return 10;
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
                return 3f;
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
                return 6;
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
                return 20;
            case PawnClass.Archer:
                return 10;
            case PawnClass.Chemist:
                return 12;
            case PawnClass.Cleric:
                return 12;
            case PawnClass.Skeleton:
                return 10;
            case PawnClass.SkeletonCPT:
                return 20;
            case PawnClass.SkeletonMage:
                return 12;
            default:
                return 1;
        }
    }

    public static int GetPawnHealth(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 50;
            case PawnClass.Archer:
                return 35;
            case PawnClass.Chemist:
                return 30;
            case PawnClass.Cleric:
                return 25;
            case PawnClass.Skeleton:
                return 35;
            case PawnClass.SkeletonCPT:
                return 50;
            case PawnClass.SkeletonMage:
                return 30;
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

public enum PawnClass
{
    Knight,
    Archer,
    Chemist,
    Cleric,
    Skeleton,
    SkeletonCPT,
    SkeletonMage
}

public enum PawnStrategy
{
    Tank,
    Flank,
    Support
}