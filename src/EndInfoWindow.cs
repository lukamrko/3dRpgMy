using Godot;
using System;

public partial class EndInfoWindow : Control
{
    Button LeftButton;
    Button RightButton;
    public override void _Ready()
    {
        this.Visible = false;
        LeftButton = GetNode<Button>("MenuBar/HBox/LeftButton");
        RightButton = GetNode<Button>("MenuBar/HBox/RightButton");
        GD.Print("Entered here");
    }

    public void SetGameLostUI()
    {
        LeftButton.Text = "Quit the game!";
        RightButton.Text = "Restart the level!";
        this.Visible = true;
        LeftButton.Pressed += QuitTheGame;
        RightButton.Pressed += RestartTheLevel;
    }

    public void SetLevelWon()
    {
        LeftButton.Text = "Restart the level!";
        RightButton.Text = "Go to the next level";
        this.Visible = true;
        LeftButton.Pressed += RestartTheLevel;
        RightButton.Pressed += GoToTheNextLevel;
    }

    public void SetGameWon()
    {
        LeftButton.Text = "Congratulations! You won the game";
        this.Visible = true;
        RightButton.Visible = false;
        LeftButton.Pressed += QuitTheGame;
    }

    private void GoToTheNextLevel()
    {
        var nextLevelInfo = LevelManager.GetNextLevelInfo();
        var nextLevelPath = nextLevelInfo.LevelPath;
        var packedScene = ResourceLoader.Load<PackedScene>(nextLevelPath);
        LevelManager.CurrentLevel++;
        GetTree().ChangeSceneToPacked(packedScene);
    }

    private void RestartTheLevel()
    {
        var currentLevelInfo = LevelManager.GetCurrentLevelInformation();
        var currentLevelPath = currentLevelInfo.LevelPath;
        var packedScene = ResourceLoader.Load<PackedScene>(currentLevelPath);
        GetTree().ChangeSceneToPacked(packedScene);
    }

    private void QuitTheGame()
    {
        GetTree().Quit();
    }
}
