using Godot;
using System;
using System.ComponentModel;


public partial class man : CharacterBody3D
{

	public CharacterBody3D player;
	[Export]
	public NodePath playerPath;
	public float speed = 5.0f;
	public double lerpVal = 8.0f;

	public bool inRange = false;
	private NavigationAgent3D navigationAgent { get; set; }
	private AnimationTree animationTree { get; set; }
	private Area3D detectionArea { get; set; }
	private Area3D hitbox { get; set; }
	private Skeleton3D skeleton { get; set; }
	enum state
	{
		chase,
		idle
	}

	state currentState = state.chase;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		player = (CharacterBody3D)GetNode(playerPath);
		navigationAgent = (NavigationAgent3D)GetNode("NavigationAgent3D");
		animationTree = (AnimationTree)GetNode("AnimationTree");
		detectionArea = (Area3D)GetNode("Area3D");
		hitbox = (Area3D)GetNode("Hitbox");
		skeleton = (Skeleton3D)GetNode("Armature/Skeleton3D");

		animationTree.Active = true;

		detectionArea.AreaEntered += (area) => goToIdle();
		detectionArea.AreaExited += (area) => goToChase();
		hitbox.AreaEntered += (area) => hit();

		//skeleton.PhysicalBonesStartSimulation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 lookPosition = new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z);
		switch(currentState)
		{
			case state.chase:
				//animationTree.Set("parameters/conditions/run", 1);
				//animationTree.Set("parameters/conditions/idle", 0);
				Velocity = Vector3.Zero;
				navigationAgent.TargetPosition = player.GlobalPosition;
        		Vector3 nextPoint = navigationAgent.GetNextPathPosition();
				Velocity = (nextPoint - GlobalPosition).Normalized() * speed;

				
                LookAt(lookPosition, Vector3.Up);

				break;
            case state.idle:
				var velocity = Velocity;
				velocity.X = (float)Mathf.Lerp(velocity.X, 0, delta * lerpVal);
				velocity.Z = (float)Mathf.Lerp(velocity.Z, 0, delta * lerpVal);
				Velocity = velocity;
				
                LookAt(lookPosition, Vector3.Up);
				//animationTree.Set("parameters/conditions/idle", 1);
				//animationTree.Set("parameters/conditions/run", 0);
				//GD.Print("asds");
				break;
			default:
				//GD.Print("adsad");
				break;
		}

		animationTree.Set("parameters/conditions/idle", inRange);
		animationTree.Set("parameters/conditions/run", !inRange);
		
		MoveAndSlide();
    }

	private void goToIdle()
	{
		inRange = true;
		currentState = state.idle;
	}

	private void goToChase()
	{
		inRange = false;
		currentState = state.chase;
	}

	private void hit()
	{
		skeleton.PhysicalBonesStartSimulation();
	}
}
