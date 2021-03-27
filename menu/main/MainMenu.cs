using Godot;
using System;

public class MainMenu : Control
{

	SceneTree tree;
	VBoxContainer title;
	HBoxContainer start;
	HBoxContainer options;

	public override void _Ready()
	{
		tree = GetTree();
		title = GetNode<VBoxContainer>("TitleScreen");
		start = GetNode<HBoxContainer>("StartGame");
		options = GetNode<HBoxContainer>("Options");
	}

	public void _on_Start_pressed()
	{
		start.Visible = true;
		title.Visible = false;
	}

	public void _on_Options_pressed()
	{
		// options.prev_menu = title;
		options.Visible = true;
		title.Visible = false;
	}

	public void _on_Exit_pressed()
	{
		tree.Quit();
	}

	public void _on_RandomBlocks_pressed()
	{
		Settings.world_type = 0;
		tree.ChangeScene("res://world/world.tscn");
	}

	public void _on_FlatGrass_pressed()
	{
			Settings.world_type = 1;
			tree.ChangeScene("res://world/world.tscn");
	}

	public void _on_BackToTitle_pressed()
	{
		title.Visible = true;
		start.Visible = false;
	}
}
