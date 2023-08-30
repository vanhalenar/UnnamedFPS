using Godot;
using System;

public partial class player : CharacterBody3D
{
	// General variables.
	public float speed = 6.0f;
	public const float walkSpeed = 6.0f;
	public const float sprintSpeed = 11.0f;
	public const float jumpVelocity = 6.0f;
	public const float sensitivity = 0.003f;
	public const float gravityMultiplier = 1.2f;
	
	// Head bob variables.
	public const float headBobFreq = 2.0f;
	public const float headBobAmpX = 0.04f;
	public const float headBobAmpY = 0.08f;
	public double headBobTime = 0.0f;

	// FOV variables.
	public const float baseFOV = 75.0f;
	public const float changeFOV = 1.5f;

	// Object picking variables.
	public PhysicalBone3D	 pickedObject;
	public int pullPower = 4;
	
	public MeshInstance3D outline;
	private Camera3D camera { get; set; }
	private RayCast3D raycast { get; set; }
	private RayCast3D aimcast { get; set; }
	private Marker3D pickupPosition { get; set; }
	private AnimationPlayer gunAnimationPlayer { get; set; }
	private Node3D muzzle { get; set; }

	//private RayCast3D gunRaycast { get; set; }

	PackedScene bulletScene;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
		raycast = GetNode<RayCast3D>("Camera3D/RayCast3D");
		aimcast = GetNode<RayCast3D>("Camera3D/AimCast");
		pickupPosition = GetNode<Marker3D>("Camera3D/PickupPosition");
		gunAnimationPlayer = GetNode<AnimationPlayer>("Camera3D/Handgun/AnimationPlayer");
		//gunRaycast = GetNode<RayCast3D>("Camera3D/Handgun/RayCast3D");
		muzzle = GetNode<Node3D>("Camera3D/Handgun/Muzzle");

		bulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");

		DisplayServer.MouseSetMode(DisplayServer.MouseMode.Captured);

	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		// Looking around.	
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * sensitivity);
			camera.RotateX(-mouseMotion.Relative.Y * sensitivity);
			
			Vector3 currentRotationDegrees = camera.RotationDegrees;
			currentRotationDegrees.X = Mathf.Clamp(currentRotationDegrees.X, -89, 89);
			camera.RotationDegrees = currentRotationDegrees; 
		}

		// Interaction.
		if (Input.IsActionJustPressed("interact"))
		{
			interactWithObject();
		}

		// Picking objects.
		
		if (Input.IsActionJustPressed("rclick"))
		{
			pickObject();
		}
		
		if (Input.IsActionJustReleased("rclick"))
		{
			dropObject();
		}
		

	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * gravityMultiplier * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = jumpVelocity;
		
		if (Input.IsActionPressed("sprint"))
		{
			speed = sprintSpeed;
		}
		else
		{
			speed = walkSpeed;
		}

		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		if (IsOnFloor())
		{
			if (direction != Vector3.Zero)
			{
				velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * speed, delta * 16.0);
				velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * speed, delta * 16.0);
			}
			else
			{
				velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * speed, delta * 13.0);
				velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * speed, delta * 13.0);
			}
		}
		else
		{
			velocity.X = (float)Mathf.Lerp(velocity.X, direction.X * speed, delta * 4.0);
			velocity.Z = (float)Mathf.Lerp(velocity.Z, direction.Z * speed, delta * 4.0);
		}
		

		Velocity = velocity;

		// Headbob.
		if (IsOnFloor())
		{
			headBobTime += delta * velocity.Length() * Convert.ToSingle(IsOnFloor());

			var transform = camera.Transform;
			transform.Origin = headbob(headBobTime);
			camera.Transform = transform;
		}

		// FOV change.
		if (Input.IsActionPressed("sprint"))
		{
			var velocityClamped = Mathf.Clamp(velocity.Length(), 0.5, sprintSpeed * 2);
			var targetFOV = baseFOV + changeFOV * velocityClamped;
			camera.Fov = (float)Mathf.Lerp(camera.Fov, targetFOV, delta * 8.0);
		}
		
		// Outline toggling.
		if (raycast.IsColliding() && raycast.GetCollider() is RigidBody3D)
		{
			//GD.Print(raycast.GetCollider());
			RigidBody3D collider = (RigidBody3D)raycast.GetCollider();
			outline = (MeshInstance3D)collider.GetChild(0).GetChild(0);
			
			if (outline != null)
			{
				outline.Visible = true;
			}
			
		}
		else
		{
			if (outline != null)
			{
				outline.Visible = false;
			}
		}

		MoveAndSlide();

		// Object picking and moving.
		if (pickedObject != null)
		{
			var a = pickedObject.GlobalTransform.Origin;
			var b = pickupPosition.GlobalTransform.Origin;
			pickedObject.LinearVelocity = (b-a)*pullPower;
			
		}

		// Shooting.
		if (Input.IsActionJustPressed("lclick"))
		{
			if (!gunAnimationPlayer.IsPlaying() && aimcast.IsColliding())
			{
				gunAnimationPlayer.Play("fire");
				var bulletInstance = (bullet)bulletScene.Instantiate();
				muzzle.AddChild(bulletInstance);
				//bulletInstance.Position = gunRaycast.GlobalPosition;
				/*var transform = bulletInstance.Transform;
				transform.Basis = gunRaycast.GlobalTransform.Basis;
				bulletInstance.Transform = transform;*/
				

				GD.Print(aimcast.GetCollisionPoint());
				//bulletInstance.GlobalPosition = aimcast.GetCollisionPoint();
				bulletInstance.LookAt(aimcast.GetCollisionPoint(), Vector3.Up);
				bulletInstance.shoot = true;
				
				

			}
		}
	}

	public void pickObject()
	{
		var collider = raycast.GetCollider();
		GD.Print(collider);
		if (collider != null && (collider is PhysicalBone3D /*|| collider is CharacterBody3D*/))
		{
			
			pickedObject = (PhysicalBone3D)collider;
		} 
	}

	public void dropObject()
	{
		if (pickedObject != null)
		{
			pickedObject = null;
		}
	}

	public void interactWithObject()
	{
		var collider = raycast.GetCollider();
		if (collider != null)
		{
			collider.Call("Test");
		}
	}

	public Vector3 headbob(double time)
	{
		Vector3 position = Vector3.Zero;
		position.Y = (float)(Mathf.Sin(time * headBobFreq) * headBobAmpY);
		position.X = (float)(Mathf.Cos(time * headBobFreq / 2) * headBobAmpX);
		return position;
	}
}
