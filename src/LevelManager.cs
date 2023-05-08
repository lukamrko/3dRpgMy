using System;
using System.Collections.Generic;
using Godot;

public static class LevelManager
{
    private static string levelConfigPath = "config/defaultLevelConfig.cfg";
    private static Dictionary<int, LevelInfo> levelInformations = new Dictionary<int, LevelInfo>();
    public static int CurrentLevel = -1;

    public static LevelInfo GetNextLevel()
    {
        return levelInformations[CurrentLevel + 1];
    }

    public static bool NextLevelExists()
    {
        return (levelInformations.ContainsKey(CurrentLevel + 1));
    }

    public static LevelInfo GetCurrentLevelInformation()
    {
        return levelInformations[CurrentLevel];
    }

    public static void LoadConfig()
    {
        var score_data = new Godot.Collections.Dictionary();
        var config = new ConfigFile();

        // Load data from a file.
        Error err = config.Load(levelConfigPath);

        // If the file didn't load, ignore it.
        if (err != Error.Ok)
        {
            return;
        }

        var firstLevel = 1;

        // Iterate over all sections.
        foreach (string level in config.GetSections())
        {
            // Fetch the data for each section.
            var roundsToWin = (int)config.GetValue(level, "RoundsToWin");
            var allowedEnemiesIndexes = (int[])config.GetValue(level, "AllowedEnemies");
            var allowedEnemies = MapAllowedEnemies(allowedEnemiesIndexes);
            var levelPath = (string)config.GetValue(level, "LevelPath");
            var levelInfo = new LevelInfo
            {
                RoundsToWin = roundsToWin,
                AllowedEnemies = allowedEnemies,
                LevelPath = levelPath
            };
            levelInformations[firstLevel++] = levelInfo;
        }
    }

    private static Godot.Collections.Array<PawnClass> MapAllowedEnemies(int[] allowedEnemiesIndexes)
    {
        var allowedEnemies = new Godot.Collections.Array<PawnClass>();
        foreach (var allowedEnemy in allowedEnemiesIndexes)
        {
            allowedEnemies.Add((PawnClass)allowedEnemy);
        }
        return allowedEnemies;
    }

    public static void CreateDefaultConfig()
    {
        var config = new ConfigFile();
        var lvl1RoundsToWin = 5;
        var lvl2RoundsToWin = 4;
        var lvl3RoundsToWin = 6;
        var lvl4RoundsToWin = 7;

        var lvl1AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher };
        var lvl2AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher, (int)PawnClass.SkeletonBomber };
        var lvl3AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher, (int)PawnClass.SkeletonBomber, (int)PawnClass.SkeletonMedic };
        var lvl4AllowedEnemies = new int[] { (int)PawnClass.SkeletonWarrior, (int)PawnClass.SkeletonArcher, (int)PawnClass.SkeletonBomber, (int)PawnClass.SkeletonMedic, (int)PawnClass.SkeletonHero };

        var lvl1Path = "res://assets/tscn/levels/Level1-FaceOfGiant.tscn";
        var lvl2Path = "res://assets/tscn/levels/Level2-Crossroads.tscn";
        var lvl3Path = "res://assets/tscn/levels/Level3-Cliffs.tscn";
        var lvl4Path = "res://assets/tscn/levels/Level4-LastStand.tscn";

        config.SetValue("level1", "RoundsToWin", lvl1RoundsToWin);
        config.SetValue("level1", "AllowedEnemies", lvl1AllowedEnemies);
        config.SetValue("level1", "LevelPath", lvl1Path);

        config.SetValue("level2", "RoundsToWin", lvl2RoundsToWin);
        config.SetValue("level2", "AllowedEnemies", lvl2AllowedEnemies);
        config.SetValue("level2", "LevelPath", lvl2Path);

        config.SetValue("level3", "RoundsToWin", lvl3RoundsToWin);
        config.SetValue("level3", "AllowedEnemies", lvl3AllowedEnemies);
        config.SetValue("level3", "LevelPath", lvl3Path);

        config.SetValue("level4", "RoundsToWin", lvl4RoundsToWin);
        config.SetValue("level4", "AllowedEnemies", lvl4AllowedEnemies);
        config.SetValue("level4", "LevelPath", lvl4Path);

        config.Save("config/defaultConfig.cfg");
    }

}