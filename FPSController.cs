using Godot;
using System;

public partial class FPSController : Node3D
{
	// Related Nodes
	// Godot doesn't allow GetNode() to be used here so it must be done in _Ready() or _Process().
	private CharacterBody3D nodeBody;
	private Node3D nodeHead;
	private Camera3D nodeFirstPersonCamera;
	
	// Physics Vars
	private Vector3 velocity = new Vector3();
	
	[Export] public const float maxSpeed = 5.0f;
	[Export] public const float acceleration = 1.0f;
	[Export] public const float jumpStrength = 4.5f;
	[Export] public const float gravity = 980.0f;
	
	// Camera Vars
	[Export] public const float cameraSensitivity = 0.006f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Getting related nodes.
		nodeBody = GetNode<CharacterBody3D>("CharacterBody3D");
		nodeHead = GetNode<Node3D>("Head");
		nodeFirstPersonCamera = GetNode<Camera3D>("Head/Camera3D");
		
		// Set mouse mode to capture mouse.
		// This makes it to where the mouse is both invisible and confined to the game window.
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Get the current velocity.
		// Vars like Velocity can be both get and set, but their components (x,y,z) cannot be set directly.
		Vector3 curVelocity = velocity;
		
		// Get the direction of movement.
		// "Move_Left", "Move_Right", etc. are all set in the project settings.
		// To be entirely honest I'm not sure what's happening when assigning movementDirection but I'll look into it at some point.
		Vector2 inputDir = Input.GetVector("Move_Left", "Move_Right", "Move_Forward", "Move_Backward");
		Vector3 movementDirection = (nodeFirstPersonCamera.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		// Set velocity...
		if(movementDirection != Vector3.Zero){	// In case the direction isn't perfectly straight.
			curVelocity.X = movementDirection.X * maxSpeed;
			curVelocity.Z = movementDirection.Z * maxSpeed;
		}else{									// In case player is looking perfectly straight.
			curVelocity.X = Mathf.MoveToward(velocity.X, 0, maxSpeed);
			curVelocity.Z = Mathf.MoveToward(velocity.Z, 0, maxSpeed);
		}
		
		// Move player.
		velocity = curVelocity;
		Position += velocity * (float)delta;
	}
	
	// Input Hook
	// This function is called anytime there is an input event.
	// Same concept as the _Ready() or _Process() function.
	public override void _Input(InputEvent @event){
		// Process mouse movement if mouse is moving.
		if(@event is InputEventMouseMotion motion){
			// Rotate head and camera horizontally.
			// Not really sure what's going on here.
			nodeHead.RotateY(-motion.Relative.X * cameraSensitivity);
			nodeFirstPersonCamera.RotateX(-motion.Relative.Y * cameraSensitivity);
			
			// Rotate camera vertically.
			Vector3 cameraRotation = nodeFirstPersonCamera.Rotation;
			cameraRotation.X = Mathf.Clamp(cameraRotation.X, Mathf.DegToRad(-80.0f), Mathf.DegToRad(80.0f));
			nodeFirstPersonCamera.Rotation = cameraRotation;
		}
	}
}
