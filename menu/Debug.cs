using Godot;
using System;
using static Godot.Mathf;

public class Debug : Label
{
Player player;
VoxelWorld voxel_world;
	public override void _Ready()
	{
		player = GetNode<Player>("../Player");
		voxel_world = GetNode<VoxelWorld>("../VoxelWorld");
	}

 public override void _Process(float delta)
 {
	if (Input.IsActionJustPressed("debug"))
		Visible = !Visible;

	// Text = "Position: " + _vector_to_string_appropriate_digits(player.Transform.origin);
	// Text += "\nEffective render distance: " + str(voxel_world.effective_render_distance);
	// Text += "\nLooking: " + _cardinal_string_from_radians(player.transform.basis.get_euler().y);
	// Text += "\nMemory: " + "%3.0f" % (OS.get_static_memory_usage() / 1048576.0) + " MiB";
	// Text += "\nFPS: " + str(Engine.get_frames_per_second());
 }

// Avoids the problem of showing more digits than needed or available.
String _vector_to_string_appropriate_digits(Vector3 vector)
{

	// var factors = [1000, 1000, 1000]
	// for i in range(3):
	// 	if abs(vector[i]) > 4096:
	// 		factors[i] = factors[i] / 10
	// 		if abs(vector[i]) > 65536:
	// 			factors[i] = factors[i] / 10
	// 			if abs(vector[i]) > 524288:
	// 				factors[i] = factors[i] / 10

	// return "(" + \
	// 		str(round(vector.x * factors[0]) / factors[0]) + ", " + \
	// 		str(round(vector.y * factors[1]) / factors[1]) + ", " + \
	// 		str(round(vector.z * factors[2]) / factors[2]) + ")";
	return null;
}


// Expects a rotation where 0 is North, on the range -PI to PI.
String _cardinal_string_from_radians(float angle)
{

	if (angle > Tau * 3 / 8)
		return "South";
	if (angle < -Tau * 3 / 8)
		return "South";
	if (angle > Tau * 1 / 8)
		return "West";
	if (angle < -Tau * 1 / 8)
		return "East";
	return "North";
}

}
