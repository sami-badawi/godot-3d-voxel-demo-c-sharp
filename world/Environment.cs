using Godot;
using System;
using static Godot.Mathf;

public class Environment : WorldEnvironment
{
	Node voxel_world;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		voxel_world = GetNodeOrNull<Node>("../VoxelWorld");
	}

	/* Need to implement a few more things before uncommenting
	*/
	public override void _Process(float delta)
	{
		Environment.FogEnabled = Settings.fog_enabled;
		Environment.DofBlurFarEnabled = Settings.fog_enabled;

		// var target_distance = Clamp(voxel_world.effective_render_distance, 2, voxel_world.render_distance - 1) * Chunk.CHUNK_SIZE;
		// float rate = delta * 4;
		// if (Environment.FogDepthEnd > target_distance) {
		// 	rate *= 2;
		// }
		// Environment.FogDepthBegin =  move_toward(Environment.FogDepthBegin, target_distance - Chunk.CHUNK_SIZE, rate);
		// Environment.FogDepthEnd = move_toward(Environment.FogDepthEnd, target_distance, rate);
		// Environment.DofBlurFarDistance = Environment.FogDepthEnd;     
	}
}
