using Godot;
using System;

public partial class MainMenu : Control
{

    private string[] _stages = new string[]
    {
		// @"res://assets/tscn/TestLevel0-Height.tscn",
        @"res://assets/tscn/levels/Level1-FaceOfGiant.tscn"
    };

    private Button _BtnNewGame;
    private Button _BtnContinue;

    private Button _BtnQuit;

    PackedScene Scene = new PackedScene();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _BtnNewGame = GetNode<Button>("VBoxContainer/NewGame");
        _BtnContinue = GetNode<Button>("VBoxContainer/Continue");
        _BtnQuit = GetNode<Button>("VBoxContainer/Quit");
        _BtnNewGame.GrabFocus();

        _BtnNewGame.Pressed += BtnNewGamePressed;
        _BtnContinue.Pressed += BtnContinuePressed;
        _BtnQuit.Pressed += BtnQuitPressed;

        LevelManager.CreateDefaultConfig();
        LevelManager.LoadConfig();

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);

    }


    public void BtnNewGamePressed()
    {
        LevelManager.CurrentLevel = 1;
        var levelPath = LevelManager.GetCurrentLevelInformation().LevelPath;
        Scene = ResourceLoader.Load<PackedScene>(levelPath);

        GetTree().ChangeSceneToPacked(Scene);
    }

    public void BtnContinuePressed()
    {
        LevelManager.CurrentLevel = LevelManager.GetCurrentLevelFromConfig();
        var levelPath = LevelManager.GetCurrentLevelInformation().LevelPath;
        Scene = ResourceLoader.Load<PackedScene>(levelPath);

        GetTree().ChangeSceneToPacked(Scene);
    }

    public void BtnQuitPressed()
    {
        GetTree().Quit();
    }

    public override void _Process(double delta)
    {
        // if(_BtnStart.Pressed)
        // {
        //     GD.Print("Start button pressed!");
        //     GetTree().ChangeSceneTo(Scene);
        // }
        // else if(_BtnQuit.Pressed)
        // {
        //     GetTree().Quit();
        // }

    }
}


