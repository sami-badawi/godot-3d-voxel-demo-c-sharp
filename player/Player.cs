using Godot;
using System;
using static Godot.Mathf;

public class Player : KinematicBody
{

public Vector3 velocity = new Vector3();

Vector2 _mouse_motion = new Vector2();
int _selected_block = 6;

float gravity;

public Spatial head;
public RayCast raycast;
public TextureRect selected_block_texture;
public VoxelWorld voxel_world;
// onready var crosshair = $"../PauseMenu/Crosshair"

	public override void _Ready()
	{
		gravity = (float) ProjectSettings.GetSetting("physics/3d/default_gravity");
		head = GetNode<Spatial>("Head");
		raycast = GetNode<RayCast>("Head/RayCast");
		selected_block_texture = GetNode<TextureRect>("SelectedBlock");
		voxel_world = GetNode<VoxelWorld>("../VoxelWorld");

		Input.SetMouseMode(Input.MouseMode.Captured);        
	}


public override void _Process(float delta)
{
	
// 	// Mouse movement.
	_mouse_motion.y = Clamp(_mouse_motion.y, -1550, 1550);
	Transform t = GlobalTransform;
	t.basis = new Basis(new Vector3(0, _mouse_motion.x * -0.001f, 0));
	GlobalTransform = t;
	// head.Transform.basis = new Basis(new Vector3(_mouse_motion.y * -0.001f, 0, 0));

// 	// Block selection.
// 	var position = raycast.get_collision_point()
// 	var normal = raycast.get_collision_normal()
// 	if Input.is_action_just_pressed("pick_block"):
// 		# Block picking.
// 		var block_global_position = (position - normal / 2).floor()
// 		_selected_block = voxel_world.get_block_global_position(block_global_position)
// 	else:
// 		# Block prev/next keys.
// 		if Input.is_action_just_pressed("prev_block"):
// 			_selected_block -= 1
// 		if Input.is_action_just_pressed("next_block"):
// 			_selected_block += 1
// 		_selected_block = wrapi(_selected_block, 1, 30)
// 	# Set the appropriate texture.
// 	var uv = Chunk.calculate_block_uvs(_selected_block)
// 	selected_block_texture.texture.region = Rect2(uv[0] * 512, Vector2.ONE * 64)

// 	// Block breaking/placing.
// 	if crosshair.visible and raycast.is_colliding():
// 		var breaking = Input.is_action_just_pressed("break")
// 		var placing = Input.is_action_just_pressed("place")
// 		# Either both buttons were pressed or neither are, so stop.
// 		if breaking == placing:
// 			return

// 		if breaking:
// 			var block_global_position = (position - normal / 2).floor()
// 			voxel_world.set_block_global_position(block_global_position, 0)
// 		elif placing:
// 			var block_global_position = (position + normal / 2).floor()
// 			voxel_world.set_block_global_position(block_global_position, _selected_block)
}


public override void _PhysicsProcess(float delta)
{
	
// 	// Crouching.
	var crouching = Input.IsActionPressed("crouch");
	// if (crouching)
	// 	head.Transform.origin = new Vector3(0, 1.2f, 0);
	// else
	// 	head.Transform.origin = new Vector3(0, 1.6f, 0);

// 	// Keyboard movement.
	var movement = Transform.basis.Xform(new Vector3(
		Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
		0,
		Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward")
	).Normalized() * (crouching ? 1 : 5));

// 	// Gravity.
	velocity.y -= gravity * delta;

// 	//warning-ignore:return_value_discarded
	velocity = MoveAndSlide(new Vector3(movement.x, velocity.y, movement.z), Vector3.Up);

// 	// Jumping, applied next frame.
	if (IsOnFloor() && Input.IsActionPressed("jump"))
		velocity.y = 5;
}


	public override void _Input(InputEvent e)
{	
	if (e is InputEventMouseMotion ev) {

		if (Input.GetMouseMode() == Input.MouseMode.Captured)
			_mouse_motion += ev.Relative;
}
}


public Vector3 chunk_pos()
{
	return (Transform.origin / Chunk.CHUNK_SIZE).Floor();	
}
	

}
