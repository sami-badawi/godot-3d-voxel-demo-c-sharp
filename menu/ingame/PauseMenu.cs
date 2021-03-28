using Godot;
using System;

public class PauseMenu : Control
{

	SceneTree tree;
	CenterContainer crosshair;
	Control pause;
	Options options;
	VoxelWorld voxel_world;

	public override void _Ready()
	{
		tree = GetTree();
		crosshair = GetNode<CenterContainer>("Crosshair");
		pause = GetNode<Control>("Pause");
		options = GetNode<Options>("Options");
		voxel_world = GetNode<VoxelWorld>("../VoxelWorld");
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("pause"))
		{
			pause.Visible = crosshair.Visible;
			crosshair.Visible = !crosshair.Visible;
			options.Visible = false;
			Input.MouseMode mode = crosshair.Visible ? Input.MouseMode.Captured : Input.MouseMode.Visible;
			Input.SetMouseMode(mode);
		}
	}

	public void _on_Resume_pressed()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
		crosshair.Visible = true;
		pause.Visible = false;
	}

	public void _on_Options_pressed()
	{
		options.prev_menu = pause;
		options.Visible = true;
		pause.Visible = false;
	}

	public void _on_MainMenu_pressed()
	{
		voxel_world.clean_up();
		tree.ChangeScene("res://menu/main/main_menu.tscn");
	}

	public void _on_Exit_pressed()
	{
		voxel_world.clean_up();
		tree.Quit();
	}
}
