using Godot;
using System;

public partial class MainMenu : Control
{

	private string [] _stages = new string [] 
	{
		@"res://assets/tscn/TestLevel0-Height.tscn",
		@"res://assets/tscn/TestLevel.tscn"
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
		// Scene = GD.Load<PackedScene>(_stages[1]);
		Scene = ResourceLoader.Load<PackedScene>(_stages[1]);

	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
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

	public void BtnStartPressed()
	{
		// GD.Print("pressed");
		GetTree().Paused = false;
		GetTree().ChangeSceneToPacked(Scene);
		// GetTree().ChangeSceneTo(Scene);

		// GetTree().ChangeSceneToPacked(Scene);
	}
}


