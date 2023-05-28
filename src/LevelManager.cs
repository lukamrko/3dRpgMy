using System.Linq;
using System;
using System.Collections.Generic;
using Godot;

public static class LevelManager
{
    private static string levelConfigPath = "config/defaultLevelConfig.cfg";
    private static string currentLevelConfigPath = "config/currentLevelConfig.cfg";

    private static Dictionary<int, LevelInfo> levelInformations = new Dictionary<int, LevelInfo>();
    private static int currentLevel = -1;

    public static int CurrentLevel 
    { 
        get => currentLevel; 
        set 
        {
            currentLevel = value;
            SetCurrentLevelInConfig(currentLevel);
        } 
    }
    public static LevelInfo GetNextLevelInfo()
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

    /// <summary>
    /// Creates default config if one doesn't exist
    /// </summary>
    public static void CreateDefaultConfig()
    {
        CreateDefaultLevelConfig();
        CreateCurrentLevelConfig();
    }

    public static int GetCurrentLevelFromConfig()
    {
        var config = new ConfigFile();

        // Load data from a file.
        Error err = config.Load(currentLevelConfigPath);

        // If the file didn't load, ignore it.
        if (err != Error.Ok)
        {
            return 1;
        }

        var currentLevel = config.GetSections().First();
        var currentLevelNumber = (int)config.GetValue(currentLevel, "currentLevel");
        return currentLevelNumber;
    }

    private static void SetCurrentLevelInConfig(int currentLevel)
    {
        var currentLevelConfig = new ConfigFile();
        Error err = currentLevelConfig.Load(currentLevelConfigPath);

        if (err != Error.Ok)
        {
            GD.Print("File not found");
            return;
        }
        currentLevelConfig.SetValue("currentLevel", "currentLevel", currentLevel);

        currentLevelConfig.Save(currentLevelConfigPath);
    }

    private static void CreateCurrentLevelConfig()
    {
        var currentLevelConfig = new ConfigFile();
        Error err = currentLevelConfig.Load(currentLevelConfigPath);

        if (err == Error.Ok)
        {
            GD.Print("File found");
            return;
        }
        currentLevelConfig.SetValue("currentLevel", "currentLevel", 1);

        currentLevelConfig.Save(currentLevelConfigPath);
    }

    private static void CreateDefaultLevelConfig()
    {
        var levelConfig = new ConfigFile();
        Error err = levelConfig.Load(levelConfigPath);

        if (err == Error.Ok)
        {
            GD.Print("File found");
            return;
        }

        var lvl1RoundsToWin = 4;
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

        levelConfig.SetValue("level1", "RoundsToWin", lvl1RoundsToWin);
        levelConfig.SetValue("level1", "AllowedEnemies", lvl1AllowedEnemies);
        levelConfig.SetValue("level1", "LevelPath", lvl1Path);

        levelConfig.SetValue("level2", "RoundsToWin", lvl2RoundsToWin);
        levelConfig.SetValue("level2", "AllowedEnemies", lvl2AllowedEnemies);
        levelConfig.SetValue("level2", "LevelPath", lvl2Path);

        levelConfig.SetValue("level3", "RoundsToWin", lvl3RoundsToWin);
        levelConfig.SetValue("level3", "AllowedEnemies", lvl3AllowedEnemies);
        levelConfig.SetValue("level3", "LevelPath", lvl3Path);

        levelConfig.SetValue("level4", "RoundsToWin", lvl4RoundsToWin);
        levelConfig.SetValue("level4", "AllowedEnemies", lvl4AllowedEnemies);
        levelConfig.SetValue("level4", "LevelPath", lvl4Path);

        levelConfig.Save(levelConfigPath);
    }
}