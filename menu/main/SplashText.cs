using Godot;
using System;
using static Godot.Mathf;

public class SplashText : Control
{
float time = 0.0f;

	public override void _Ready()
	{
		GD.Print("SplashText Ready");
	}

	public override void _Process(float delta)
	{
		time += delta;
		RectScale = Vector2.One * (1 - Abs(Sin(time * 4)) / 4);
	}
}
