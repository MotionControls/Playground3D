using Godot;
using System;

public partial class FPSController : Node3D
{
	// Related Nodes
	// Godot doesn't allow GetNode() to be used here so it must be done in _Ready() or _Process().
	private CharacterBody3D nodeBody;
	private Node3D nodeHead;
	private Camera3D nodeFirstPersonCamera;
	private Label playerStatDisplay;
	
	// Physics Vars
	private Vector3 velocity = new Vector3();
	private Vector2 previousInputDir = new Vector2();
	private float curSpeed = 0.0f;
	
	// Exported variables CANNOT be constants, or they don't show up in the editor.
	[Export] public float maxSpeed = 5.0f;
	[Export] public float acceleration = 1.0f;
	[Export] public float jumpStrength = 4.5f;
	[Export] public float gravity = 980.0f;
		
	// Camera Vars
	[Export] public float cameraSensitivity = 0.006f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Getting related nodes.
		nodeBody = GetNode<CharacterBody3D>("CharacterBody3D");
		nodeHead = GetNode<Node3D>("Head");
		nodeFirstPersonCamera = GetNode<Camera3D>("Head/Camera3D");
		playerStatDisplay = GetParent().GetParent().GetNode<Label>("HUD/PlayerStatDisplay");
		
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
		// If no "desired movement" is detected, rely on the last known desired direction for sake of acceleration.
		// "Move_Left", "Move_Right", etc. are all set in the project settings.
		bool movementDesired = true;
		Vector2 inputDir = Input.GetVector("Move_Left", "Move_Right", "Move_Forward", "Move_Backward");
		if(inputDir.IsZeroApprox()){
			inputDir = previousInputDir;
			movementDesired = false;
		}
		previousInputDir = inputDir;
		
		// To be entirely honest I'm not sure what's happening when assigning movementDirection but I'll look into it at some point.
		Vector3 movementDirection = (nodeFirstPersonCamera.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		// Add or subtract from current speed.
		// Also clamp between 0 and max speed.
		curSpeed = Mathf.Clamp(
			(!movementDesired) ? (curSpeed - acceleration) : (curSpeed + acceleration),
			0.0f,
			maxSpeed
		);
		
		// Set velocity...
		if(movementDirection != Vector3.Zero){	// In case the direction isn't perfectly straight.
			curVelocity.X = movementDirection.X * curSpeed;
			curVelocity.Z = movementDirection.Z * curSpeed;
		}else{									// In case player is looking perfectly straight.
			curVelocity.X = Mathf.MoveToward(velocity.X, 0, curSpeed);
			curVelocity.Z = Mathf.MoveToward(velocity.Z, 0, curSpeed);
		}
		
		// Move player.
		velocity = curVelocity;
		Position += velocity * (float)delta;
		
		// Display player stats.
		playerStatDisplay.Text = "Speed: " + curSpeed.ToString() + " / " + maxSpeed.ToString() + "\nAcceleration: " + acceleration.ToString();
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
