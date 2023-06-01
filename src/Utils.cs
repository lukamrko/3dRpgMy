using Godot;
using System;

public static class Utils
{
    #region Paths
    private static string KnightSprite = "res://assets/sprites/characters/chr_pawn_knight.png";
    private static string ArcherSprite = "res://assets/sprites/characters/chr_pawn_archer.png";
    private static string ChemistSprite = "res://assets/sprites/characters/chr_pawn_chemist.png";
    private static string ClericSprite = "res://assets/sprites/characters/chr_pawn_mage.png";
    private static string SkeletonHeroSprite = "res://assets/sprites/characters/chr_pawn_skeleton_hero.png";
    private static string SkeletonSprite = "res://assets/sprites/characters/chr_pawn_skeleton.png";
    private static string SkeletonArcherSprite = "res://assets/sprites/characters/chr_pawn_skeleton_archer.png";
    private static string SkeletonMedicSprite = "res://assets/sprites/characters/chr_pawn_skeleton_medic.png";
    private static string SkeletonBomberSprite = "res://assets/sprites/characters/chr_pawn_skeleton_bomber.png";
    // private static string SkeletonMedicSprite = "res://assets/sprites/characters/chr_pawn_skeleton_medic.png";
    private static string TotemSprite = "res://assets/sprites/characters/totem.png";

    private static string SoundArcherAttack = "res://assets/sound/attacks/atk_archer.wav";
    private static string SoundChemistAttack = "res://assets/sound/attacks/atk_chemist.wav";
    private static string SoundKnightAttack = "res://assets/sound/attacks/atk_knight.wav";
    private static string SoundSkeletonAttack = "res://assets/sound/attacks/atk_ske_enemy.wav";
    private static string SoundSkeletonHeal = "res://assets/sound/attacks/atk_ske_heal.wav";


    static string TileSrc = "res://src/Tile.cs";
#endregion

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
    public static void ConvertTilesIntoStaticBodies(Node3D tilesObj)
    {
        var script = ResourceLoader.Load<RefCounted>(TileSrc);
        var tilesObjects1 = tilesObj.GetChildren();

        var tilesObjects = tilesObj.GetChildren().As<MeshInstance3D>();

        foreach (MeshInstance3D tileObj in tilesObjects)
        {
            tileObj.CreateTrimeshCollision();
            StaticBody3D staticBody = tileObj.GetChild(0) as StaticBody3D;
            staticBody.Position = tileObj.Position;

            tileObj.Position = Vector3.Zero;
            tileObj.Name = "Tile";
            tileObj.RemoveChild(staticBody);
            tilesObj.RemoveChild(tileObj);

            staticBody.AddChild(tileObj);
            var instanceID = staticBody.GetInstanceId();

            staticBody.SetScript(script);
            Tile staticBodyTile = GodotObject.InstanceFromId(instanceID) as Tile;
            staticBodyTile._Ready();
            staticBodyTile.ConfigureTile();
            staticBodyTile.SetProcess(true);
            tilesObj.AddChild(staticBodyTile);
        }
    }

    public static StandardMaterial3D CreateMaterial(Color color, Texture2D texture = null)
    {
        StandardMaterial3D material = new StandardMaterial3D();
        //TODO Check if this is the right way to do things
        // material.FlagsTransparent = true;
        material.Transparency = BaseMaterial3D.TransparencyEnum.Max;
        material.AlbedoColor = color;
        material.AlbedoTexture = texture;
        return material;
    }

    public static AudioStream GetPawnAttackSound(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return GD.Load<AudioStream>(SoundKnightAttack);
            case PawnClass.Archer:
                return GD.Load<AudioStream>(SoundArcherAttack);
            case PawnClass.Chemist:
                return GD.Load<AudioStream>(SoundChemistAttack);
            case PawnClass.SkeletonWarrior:
                return GD.Load<AudioStream>(SoundSkeletonAttack);
            case PawnClass.SkeletonArcher:
                return GD.Load<AudioStream>(SoundSkeletonAttack);
            case PawnClass.SkeletonBomber:
                return GD.Load<AudioStream>(SoundSkeletonAttack);
            case PawnClass.SkeletonHero:
                return GD.Load<AudioStream>(SoundSkeletonAttack);
            case PawnClass.SkeletonMedic:
                return GD.Load<AudioStream>(SoundSkeletonHeal);
            default:
                return GD.Load<AudioStream>(SoundSkeletonAttack);
        }
    }

    public static Texture2D GetPawnSprite(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return ResourceLoader.Load(KnightSprite) as Texture2D;
            case PawnClass.Archer:
                return ResourceLoader.Load(ArcherSprite) as Texture2D;
            case PawnClass.Chemist:
                return ResourceLoader.Load(ChemistSprite) as Texture2D;
            case PawnClass.Cleric:
                return ResourceLoader.Load(ClericSprite) as Texture2D;
            case PawnClass.Totem:
                return ResourceLoader.Load(TotemSprite) as Texture2D;
            case PawnClass.SkeletonWarrior:
                return ResourceLoader.Load(SkeletonSprite) as Texture2D;
            case PawnClass.SkeletonArcher:
                return ResourceLoader.Load(SkeletonArcherSprite) as Texture2D;
            case PawnClass.SkeletonBomber:
                return ResourceLoader.Load(SkeletonBomberSprite) as Texture2D;
            case PawnClass.SkeletonHero:
                return ResourceLoader.Load(SkeletonHeroSprite) as Texture2D;
            case PawnClass.SkeletonMedic:
                return ResourceLoader.Load(SkeletonMedicSprite) as Texture2D;
            default:
                return ResourceLoader.Load(KnightSprite) as Texture2D;
        }
    }

    public static int GetPawnMoveRadius(PawnClass pawnClass)
    {
        switch (pawnClass)
        {
            case PawnClass.Knight:
                return 3;
            case PawnClass.Archer:
                return 6;
            case PawnClass.Chemist:
                return 5;
            case PawnClass.Cleric:
                return 4;
            case PawnClass.Totem:
                return 0;
            case PawnClass.SkeletonWarrior:
                return 10;
            case PawnClass.SkeletonArcher:
                return 12;
            case PawnClass.SkeletonBomber:
                return 99;
            case PawnClass.SkeletonHero:
                return 6;
            case PawnClass.SkeletonMedic:
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
                return 1.5f;
            case PawnClass.Archer:
                return 3f;
            case PawnClass.Chemist:
                return 2f;
            case PawnClass.Cleric:
                return 1f;
            case PawnClass.Totem:
                return 0;
            case PawnClass.SkeletonWarrior:
                return 1.5f;
            case PawnClass.SkeletonArcher:
                return 4f;
            case PawnClass.SkeletonBomber:
                return 10f;
            case PawnClass.SkeletonHero:
                return 1.5f;
            case PawnClass.SkeletonMedic:
                return 1.5f;
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
                return 4;
            case PawnClass.Cleric:
                return 3;
            case PawnClass.Totem:
                return 0;
            case PawnClass.SkeletonWarrior:
                return 1;
            case PawnClass.SkeletonArcher:
                return 4;
            case PawnClass.SkeletonBomber:
                return 1;
            case PawnClass.SkeletonHero:
                return 1;
            case PawnClass.SkeletonMedic:
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
            case PawnClass.Totem:
                return 0;
            case PawnClass.SkeletonWarrior:
                return 1;
            case PawnClass.SkeletonArcher:
                return 1;
            case PawnClass.SkeletonHero:
                return 3;
            case PawnClass.SkeletonMedic:
                return 0;
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
            case PawnClass.Totem:
                return 2;
            case PawnClass.SkeletonWarrior:
                return 4;
            case PawnClass.SkeletonArcher:
                return 1;
            case PawnClass.SkeletonBomber:
                return 2;
            case PawnClass.SkeletonHero:
                return 9;
            case PawnClass.SkeletonMedic:
                return 3;
            default:
                return 1;
        }
    }

    public static Vector3 VectorRemoveY(Vector3 vector)
    {
        vector.Y = 0;
        return vector;
    }

    public static float VectorDistanceWithoutY(Vector3 a, Vector3 b)
    {
        return VectorRemoveY(a).DistanceTo(VectorRemoveY(b));
    }
}