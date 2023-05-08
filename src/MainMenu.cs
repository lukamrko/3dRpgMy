using Godot;
using System;

public partial class MainMenu : Control
{

    private string[] _stages = new string[]
    {
		// @"res://assets/tscn/TestLevel0-Height.tscn",
        @"res://assets/tscn/levels/Level1-FaceOfGiant.tscn"
    };

    private Button _BtnStart;
    private Button _BtnQuit;

    PackedScene Scene = new PackedScene();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _BtnStart = GetNode<Button>("VBoxContainer/Start");
        _BtnQuit = GetNode<Button>("VBoxContainer/Quit");
        _BtnStart.GrabFocus();

        _BtnStart.Pressed += BtnStartPressed;
        _BtnQuit.Pressed += BtnQuitPressed;

        LevelManager.CreateDefaultConfig();
        LevelManager.LoadConfig();
        LevelManager.CurrentLevel = 1;
		var levelPath = LevelManager.GetCurrentLevelInformation().LevelPath;
        Scene = ResourceLoader.Load<PackedScene>(levelPath);
    }


    public void BtnStartPressed()
    {
        // GD.Print("pressed");
        // GetTree().Paused = false;
        // var x = Scene.ResourceLocalToScene = true;
        // var level = Scene.GetLocalScene() as Level;
        // level.RoundWhenPlayerWins = 2;
        GetTree().ChangeSceneToPacked(Scene);
        // GetTree().ChangeSceneTo(Scene);

        // GetTree().ChangeSceneToPacked(Scene);
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


