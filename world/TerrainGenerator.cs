using Godot;
using Godot.Collections;
using System;
using System.Linq;

public class TerrainGenerator : Resource
{

	const int CHUNK_SIZE = 16;


	static Dictionary<Vector3, int> empty()
	{
		var random_data = new Dictionary<Vector3, int>();
		return random_data;
	}


	static public Dictionary<Vector3, int> random_blocks()
	{
		var random_data = new Dictionary<Vector3, int>();
		foreach (int x in Enumerable.Range(0, CHUNK_SIZE))
		{
			foreach (int y in Enumerable.Range(0, CHUNK_SIZE))
			{
				foreach (int z in Enumerable.Range(0, CHUNK_SIZE))
				{
					var vec = new Vector3(x, y, z);
					if (GD.Randf() < 0.01)
						random_data[vec] = (int) GD.Randi() % 29 + 1;
				}
			}
		}
		return random_data;
	}

	public static Dictionary<Vector3, int> flat(Vector3 chunk_position)
	{
		var data = new Dictionary<Vector3, int>();

		if (chunk_position.y != -1)
			return data;

		foreach (int x in Enumerable.Range(0, CHUNK_SIZE))
		{
			foreach (int z in Enumerable.Range(0, CHUNK_SIZE))
			{
				data[new Vector3(x, 0, z)] = 3;
			}
		}
		return data;
	}


	// Used to create the project icon.
	static Dictionary origin_grass(Vector3 chunk_position)
	{
		var dictionary = new Dictionary();
		if (chunk_position == Vector3.Zero)
			dictionary[Vector3.Zero] = 3;
		return dictionary;
	}
}
