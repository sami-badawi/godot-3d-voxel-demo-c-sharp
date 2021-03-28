using Godot;
using System;

public class Options : HBoxContainer
{
	public Control prev_menu { get; set; }

	public override void _Ready()
	{
		prev_menu = GetNodeOrNull<Control>("../TitleScreen");
	}

	public void _on_Back_pressed()
	{
		if (prev_menu != null)
			prev_menu.Visible = true;
		Visible = false;
	}
}
