using Godot;
using Godot.Collections;
using System.Collections.Generic;
using static Godot.Mathf;
using System;
using System.Linq;

/// <summary>
/// This file manages the creation and deletion of Chunks.
/// </summary>
public class VoxelWorld : Node
{
	Vector3 CHUNK_MIDPOINT = new Vector3(0.5f, 0.5f, 0.5f) * Chunk.CHUNK_SIZE;
	const float CHUNK_END_SIZE = Chunk.CHUNK_SIZE - 1;

	float _render_distance;
	public float render_distance
	{
		get => _render_distance;
		set
		{
			_render_distance = value;
			_delete_distance = value + 2;
		}
	} //setget _set_render_distance
	float _delete_distance = 0;
	public int effective_render_distance = 0;
	Vector3 _old_player_chunk = new Vector3(); // TODO: Vector3i

	bool _generating = true;
	bool _deleting = false;

	Godot.Collections.Dictionary<Vector3, Chunk> _chunks = new Godot.Collections.Dictionary<Vector3, Chunk>();

	Player player;
	public override void _Ready()
	{
		player = GetNode<Player>("../Player");
	}

	IEnumerable<int> makeRange(int middle)
	{
		return Enumerable.Range(middle - effective_render_distance, 2 * effective_render_distance);
	}

	public override void _Process(float delta)
	{
		render_distance = Settings.render_distance;
		var player_chunk = (player.Transform.origin / Chunk.CHUNK_SIZE).Round();

		if (_deleting || player_chunk != _old_player_chunk)
		{
			_delete_far_away_chunks(player_chunk);
			_generating = true;
		}

		if (!_generating)
			return;

		// Try to generate chunks ahead of time based on where the player is moving.
		player_chunk.y += Round(Clamp(player.velocity.y, -render_distance / 4, render_distance / 4));

		// Check existing chunks within range. If it doesn't exist, create it.
		foreach (int x in makeRange((int)player_chunk.x))
		{
			foreach (int y in makeRange((int)player_chunk.y))
			{
				foreach (int z in makeRange((int)player_chunk.z))
				{
					var chunk_position = new Vector3(x, y, z);
					if (player_chunk.DistanceTo(chunk_position) > render_distance)
						continue;

					if (_chunks.ContainsKey(chunk_position))
						continue;

					Chunk chunk = new Chunk();
					chunk.chunk_position = chunk_position;
					_chunks[chunk_position] = chunk;
					AddChild(chunk);
					return;
				}
			}
		}

		// If we didn't generate any chunks (and therefore didn't return), what next?
		if (effective_render_distance < render_distance)
			// We can move on to the next stage by increasing the effective distance.
			effective_render_distance += 1;
		else
			// Effective render distance is maxed out, done generating.
			_generating = false;
	}

	public int get_block_global_position(Vector3 block_global_position)
	{
		var chunk_position = (block_global_position / Chunk.CHUNK_SIZE).Floor();
		if (_chunks.ContainsKey(chunk_position))
		{
			var chunk = _chunks[chunk_position];
			var sub_position = block_global_position.PosMod(Chunk.CHUNK_SIZE);
			 if (chunk.data.ContainsKey(sub_position))
			 	return chunk.data[sub_position];
		}
		return 0;
	}

	public void set_block_global_position(Vector3 block_global_position, int block_id)
	{
		var chunk_position = (block_global_position / Chunk.CHUNK_SIZE).Floor();
		var chunk = _chunks[chunk_position];
		var sub_position = block_global_position.PosMod(Chunk.CHUNK_SIZE);
		if (block_id == 0)
			chunk.data.Remove(sub_position);
		else
			chunk.data[sub_position] = block_id;
		chunk.regenerate();

		// 	# We also might need to regenerate some neighboring chunks.
		if (Chunk.is_block_transparent(block_id))
		{
			if (sub_position.x == 0)
				_chunks[chunk_position + Vector3.Left].regenerate();
			else if (sub_position.x == CHUNK_END_SIZE)
				_chunks[chunk_position + Vector3.Right].regenerate();
			if (sub_position.z == 0)
				_chunks[chunk_position + Vector3.Forward].regenerate();
			else if (sub_position.z == CHUNK_END_SIZE)
				_chunks[chunk_position + Vector3.Back].regenerate();
			if (sub_position.y == 0)
				_chunks[chunk_position + Vector3.Down].regenerate();
			else if (sub_position.y == CHUNK_END_SIZE)
				_chunks[chunk_position + Vector3.Up].regenerate();
		}
	}

	public void clean_up()
	{
		foreach (var chunk_position_key in _chunks.Keys)
		{
			var thread = _chunks[chunk_position_key]._thread;
			if (thread != null)
				thread.WaitToFinish();
		}
		_chunks = new Godot.Collections.Dictionary<Vector3, Chunk>();
		SetProcess(false);
		foreach (var c in GetChildren())
		{
			if (c is Node node)
				node.Free();
		}
	}

	void _delete_far_away_chunks(Vector3 player_chunk)
	{
		_old_player_chunk = player_chunk;
		// If we need to delete chunks, give the new chunk system a chance to catch up.
		effective_render_distance = Max(1, effective_render_distance - 1);

		int deleted_this_frame = 0;
		// We should delete old chunks more aggressively if moving fast.
		// An easy way to calculate this is by using the effective render distance.
		// The specific values in this formula are arbitrary and from experimentation.
		var max_deletions = Clamp(2 * (render_distance - effective_render_distance), 2, 8);
		// Also take the opportunity to delete far away chunks.
		foreach (var chunk_position_key in _chunks.Keys)
		{
			if (player_chunk.DistanceTo(chunk_position_key) > _delete_distance)
			{
				var thread = _chunks[chunk_position_key]._thread;
				if (thread != null)
					thread.WaitToFinish();
				_chunks[chunk_position_key].QueueFree();
				_chunks.Remove(chunk_position_key);
				deleted_this_frame += 1;
				// Limit the amount of deletions per frame to avoid lag spikes.
				if (deleted_this_frame > max_deletions)
				{
					// Continue deleting next frame.
					_deleting = true;
					return;
				}
			}
		}

		// We're done deleting.
		_deleting = false;
	}
}
