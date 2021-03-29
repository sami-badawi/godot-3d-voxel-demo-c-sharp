using Godot;
using System;
using Godot.Collections;

public class Chunk : StaticBody
{
	public const int CHUNK_SIZE = 16; // Keep in sync with TerrainGenerator.
	const int TEXTURE_SHEET_WIDTH = 8;

	const int CHUNK_LAST_INDEX = CHUNK_SIZE - 1;
	const float TEXTURE_TILE_SIZE = 1.0f / TEXTURE_SHEET_WIDTH;

	public Dictionary<Vector3, int> data = new Dictionary<Vector3, int>();
	public Vector3 chunk_position = new Vector3(); // TODO: Vector3i

	public Godot.Thread _thread;

	VoxelWorld voxel_world;

	public override void _Ready()
	{
		voxel_world = GetParent() as VoxelWorld;
		var t = GlobalTransform;
		t.origin = chunk_position * CHUNK_SIZE;
		GlobalTransform = t;
		Name = chunk_position.ToString();

		if (Settings.world_type == 0)
		{
			data = TerrainGenerator.random_blocks();
		}
		else
		{
			data = TerrainGenerator.flat(chunk_position);
		}
		// We can only add colliders in the main thread due to physics limitations.
		_generate_chunk_collider();

		// However, we can use a thread for mesh generation.
		_thread = new Godot.Thread();
		_thread.Start(this, "_generate_chunk_mesh");
	}

	public void regenerate()
	{
		// Clear out all old nodes first.
		foreach (var c in GetChildren())
		{
			if (c is Node node)
			{
				RemoveChild(node);
				node.QueueFree();
			}
		}

		// Then generate new ones.
		_generate_chunk_collider();
		_generate_chunk_mesh(0);
	}

	void _generate_chunk_collider()
	{
		if (0 < data.Count)
		{
			// Avoid errors caused by StaticBody not having colliders.
			_create_block_collider(Vector3.Zero);
		}
		CollisionLayer = 0;
		CollisionMask = 0;
		return;

		// For each block, generate a collider. Ensure collision layers are enabled.
		CollisionLayer = 0xFFFFF;
		CollisionMask = 0xFFFFF;
				foreach (var block_position in data.Keys) {
					var block_id = data[block_position];
				if (block_id != 27 && block_id != 28)
						_create_block_collider(block_position);
			}
	}

	void _generate_chunk_mesh(int _this_argument_exists_due_to_bug_9924)
	{
		if (0 < data.Count)
			return;

		var surface_tool = new SurfaceTool();
		surface_tool.Begin(Mesh.PrimitiveType.Triangles);

		// For each block, add data to the SurfaceTool and generate a collider.
		foreach (var block_position in data.Keys)
		{
			var block_id = data[block_position];
			_draw_block_mesh(surface_tool, block_position, block_id);

			// Create the chunk's mesh from the SurfaceTool data.
			surface_tool.GenerateNormals();
			surface_tool.GenerateTangents();

			surface_tool.Index();
			var array_mesh = surface_tool.Commit();

			var mi = new MeshInstance();
			mi.Mesh = array_mesh;
			mi.MaterialOverride = ResourceLoader.Load("res://world/textures/material.tres") as Material;

			AddChild(mi);
		}
	}


	void _draw_block_mesh(SurfaceTool surface_tool, Vector3 block_sub_position, int block_id)
	{
		var verts = calculate_block_verts(block_sub_position);
		var uvs = calculate_block_uvs(block_id);
		var top_uvs = uvs;
		var bottom_uvs = uvs;

		// Bush blocks get drawn in their own special way.
		if (block_id == 27 || block_id == 28)
		{
			_draw_block_face(surface_tool, new Array<Vector3> { verts[2], verts[0], verts[7], verts[5] }, uvs);
			_draw_block_face(surface_tool, new Array<Vector3> { verts[7], verts[5], verts[2], verts[0] }, uvs);
			_draw_block_face(surface_tool, new Array<Vector3> { verts[3], verts[1], verts[6], verts[4] }, uvs);
			_draw_block_face(surface_tool, new Array<Vector3> { verts[6], verts[4], verts[3], verts[1] }, uvs);
			return;
		}

		// Allow some blocks to have different top/bottom textures.
		if (block_id == 3) // Grass.
		{
			top_uvs = calculate_block_uvs(0);
			bottom_uvs = calculate_block_uvs(2);
		}
		else if (block_id == 5) // Furnace.
		{
			top_uvs = calculate_block_uvs(31);
			bottom_uvs = top_uvs;
		}
		else if (block_id == 12) // Log.
		{
			top_uvs = calculate_block_uvs(30);
			bottom_uvs = top_uvs;
		}
		else if (block_id == 19) // Bookshelf.
		{
			top_uvs = calculate_block_uvs(4);
			bottom_uvs = top_uvs;
		}

		// Main rendering code for normal blocks.
		var other_block_position = block_sub_position + Vector3.Left;
		var other_block_id = 0;
		if (other_block_position.x == -1)
			other_block_id = voxel_world.get_block_global_position(other_block_position + chunk_position * CHUNK_SIZE);
		else if (data.ContainsKey(other_block_position))
			other_block_id = data[other_block_position];
		if (block_id != other_block_id && is_block_transparent(other_block_id))
		{
			_draw_block_face(surface_tool, new Array<Vector3> { verts[2], verts[0], verts[3], verts[1] }, uvs);
			other_block_position = block_sub_position + Vector3.Right;
			other_block_id = 0;
		}
		if (other_block_position.x == CHUNK_SIZE)
		{
			other_block_id = voxel_world.get_block_global_position(other_block_position + chunk_position * CHUNK_SIZE);
		}
		else if (data.ContainsKey(other_block_position))
		{
			other_block_id = data[other_block_position];
		}
		if (block_id != other_block_id && is_block_transparent(other_block_id))
		{
			_draw_block_face(surface_tool, new Array<Vector3> { verts[7], verts[5], verts[6], verts[4] }, uvs);
			other_block_position = block_sub_position + Vector3.Forward;
			other_block_id = 0;
		}

		if (other_block_position.z == -1)
			other_block_id = voxel_world.get_block_global_position(other_block_position + chunk_position * CHUNK_SIZE);

		else if (data.ContainsKey(other_block_position))
		{
			other_block_id = data[other_block_position];
		}

		if (block_id != other_block_id && is_block_transparent(other_block_id))
			_draw_block_face(surface_tool, new Array<Vector3> { verts[6], verts[4], verts[2], verts[0] }, uvs);

		other_block_position = block_sub_position + Vector3.Back;
		other_block_id = 0;
		if (other_block_position.z == CHUNK_SIZE)
			other_block_id = voxel_world.get_block_global_position(other_block_position + chunk_position * CHUNK_SIZE);
		else if (data.ContainsKey(other_block_position))
			other_block_id = data[other_block_position];
		if (block_id != other_block_id && is_block_transparent(other_block_id))
			_draw_block_face(surface_tool, new Array<Vector3> { verts[3], verts[1], verts[7], verts[5] }, uvs);
		other_block_position = block_sub_position + Vector3.Down;
		other_block_id = 0;
		if (other_block_position.y == -1)
			other_block_id = voxel_world.get_block_global_position(other_block_position + chunk_position * CHUNK_SIZE);
		else if (data.ContainsKey(other_block_position))
			other_block_id = data[other_block_position];
		if (block_id != other_block_id && is_block_transparent(other_block_id))
			_draw_block_face(surface_tool, new Array<Vector3> { verts[4], verts[5], verts[0], verts[1] }, bottom_uvs);
		other_block_position = block_sub_position + Vector3.Up;
		other_block_id = 0;
		if (other_block_position.y == CHUNK_SIZE)
			other_block_id = voxel_world.get_block_global_position(other_block_position + chunk_position * CHUNK_SIZE);
		else if (data.ContainsKey(other_block_position))
			other_block_id = data[other_block_position];
		if (block_id != other_block_id && is_block_transparent(other_block_id))
			_draw_block_face(surface_tool, new Array<Vector3> { verts[2], verts[3], verts[6], verts[7] }, top_uvs);
	}

	void _draw_block_face(SurfaceTool surface_tool,
	Godot.Collections.Array<Vector3> verts,
	Godot.Collections.Array<Vector2> uvs)
	{
		surface_tool.AddUv(uvs[1]); surface_tool.AddVertex(verts[1]);
		surface_tool.AddUv(uvs[2]); surface_tool.AddVertex(verts[2]);
		surface_tool.AddUv(uvs[3]); surface_tool.AddVertex(verts[3]);
		surface_tool.AddUv(uvs[2]); surface_tool.AddVertex(verts[2]);
		surface_tool.AddUv(uvs[1]); surface_tool.AddVertex(verts[1]);
		surface_tool.AddUv(uvs[0]); surface_tool.AddVertex(verts[0]);
	}

	void _create_block_collider(Vector3 block_sub_position)
	{
		var collider = new CollisionShape();

		var boxShape = new BoxShape();
		boxShape.Extents = Vector3.One / 2;
		collider.Shape = boxShape;
		var t = collider.GlobalTransform;
		t.origin = block_sub_position + Vector3.One / 2;
		collider.GlobalTransform = t;
		AddChild(collider);
	}

	public static Godot.Collections.Array<Vector2> calculate_block_uvs(int block_id)
	{
		// This method only supports square texture sheets.
		var row = block_id / TEXTURE_SHEET_WIDTH;

		var col = block_id % TEXTURE_SHEET_WIDTH;

		var textures = new Array<Vector2> {
				TEXTURE_TILE_SIZE * new Vector2(col, row),
				TEXTURE_TILE_SIZE * new Vector2(col, row + 1),
				TEXTURE_TILE_SIZE * new Vector2(col + 1, row),
				TEXTURE_TILE_SIZE * new Vector2(col + 1, row + 1),
			};
		return textures;
	}

	public static Godot.Collections.Array<Vector3> calculate_block_verts(Vector3 block_position)
	{
		var vertexes = new Array<Vector3>
			{
				new Vector3(block_position.x, block_position.y, block_position.z),
				new Vector3(block_position.x, block_position.y, block_position.z + 1),
				new Vector3(block_position.x, block_position.y + 1, block_position.z),
				new Vector3(block_position.x, block_position.y + 1, block_position.z + 1),
				new Vector3(block_position.x + 1, block_position.y, block_position.z),
				new Vector3(block_position.x + 1, block_position.y, block_position.z + 1),
				new Vector3(block_position.x + 1, block_position.y + 1, block_position.z),
				new Vector3(block_position.x + 1, block_position.y + 1, block_position.z + 1)
			};
		return vertexes;
	}

	public static bool is_block_transparent(int block_id)
	{
		return block_id == 0 || (block_id > 25 && block_id < 30);
	}
}
