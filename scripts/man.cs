using Godot;
using System;
using System.ComponentModel;


public partial class man : CharacterBody3D
{

	public CharacterBody3D player;
	[Export]
	public NodePath playerPath;
	public float runSpeed = 5.0f;
	public float walkSpeed = 3.0f;
	public double lerpVal = 8.0f;
	public Vector3 locationA;
	public Vector3 locationB;
	public Vector3 nextLocation;

	public bool inRange = false;
	private NavigationAgent3D navigationAgent { get; set; }
	private AnimationTree animationTree { get; set; }
	private AnimationPlayer animationPlayer { get; set; }
	private Area3D detectionArea { get; set; }
	private Area3D shootingRange { get; set; }
	private Area3D hitbox { get; set; }
	private Skeleton3D skeleton { get; set; }
	private Marker3D pointA { get; set; }
	private Marker3D pointB { get; set; }
	private Timer patrolBreakTimer { get; set; }
	enum state
	{
		chase,
		idle,
		dead,
		attack,
		patrol
	}

	state currentState = state.patrol;
	


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		player = (CharacterBody3D)GetNode(playerPath);
		navigationAgent = (NavigationAgent3D)GetNode("NavigationAgent3D");
		animationTree = (AnimationTree)GetNode("AnimationTree");
		detectionArea = (Area3D)GetNode("DetectionArea");
		shootingRange = (Area3D)GetNode("ShootingRange");
		hitbox = (Area3D)GetNode("Hitbox");
		skeleton = (Skeleton3D)GetNode("Armature/Skeleton3D");
		pointA = (Marker3D)GetNode("PointA");
		pointB = (Marker3D)GetNode("PointB");
		patrolBreakTimer = (Timer)GetNode("PatrolBreakTimer");
		animationPlayer = (AnimationPlayer)GetNode("AnimationPlayer");

		locationA = pointA.GlobalPosition;
		locationB = pointB.GlobalPosition;
		nextLocation = locationB;

		//animationTree.Active = true;
		animationPlayer.Play("walk", 0.2);

		detectionArea.AreaEntered += (area) => playerDetected();
		shootingRange.AreaEntered += (area) => playerInRange();
		shootingRange.AreaExited += (area) => playerOutOfRange();
		
		patrolBreakTimer.Timeout += () => patrolBreakTimeout();
		hitbox.AreaEntered += (area) => hit();

	}

    


    // Called every frame. 'delta' is the elapsed time since the previous frame.

    public override void _Process(double delta)
	{
		
		switch(currentState)
		{
			case state.patrol:
				Velocity = Vector3.Zero;
				Vector3 lookPosition;

				if ((nextLocation - GlobalPosition).Length()<0.5f)
				{
					patrolBreakTimer.Start();
					currentState = state.idle;
					animationPlayer.Play("idle", 0.2);
					switchDestination();
				}
				else
				{
					Velocity = (nextLocation - GlobalPosition).Normalized() * walkSpeed;
					lookPosition = new Vector3(GlobalPosition.X + Velocity.X, GlobalPosition.Y, GlobalPosition.Z + Velocity.Z);
				
                	LookAt(lookPosition, Vector3.Up);
				}

				
				break;
				
			case state.chase:
				Velocity = Vector3.Zero;
				navigationAgent.TargetPosition = player.GlobalPosition;
        		Vector3 nextPoint = navigationAgent.GetNextPathPosition();
				Velocity = (nextPoint - GlobalPosition).Normalized() * runSpeed;
				lookPosition = new Vector3(GlobalPosition.X + Velocity.X, GlobalPosition.Y, GlobalPosition.Z + Velocity.Z);
				
                LookAt(lookPosition, Vector3.Up);

				break;

            case state.idle:
				var velocity = Velocity;
				velocity.X = (float)Mathf.Lerp(velocity.X, 0, delta * lerpVal);
				velocity.Z = (float)Mathf.Lerp(velocity.Z, 0, delta * lerpVal);
				Velocity = velocity;

				break;

			case state.attack:
				velocity = Velocity;
				velocity.X = (float)Mathf.Lerp(velocity.X, 0, delta * lerpVal);
				velocity.Z = (float)Mathf.Lerp(velocity.Z, 0, delta * lerpVal);
				Velocity = velocity;

				lookPosition = new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z);
				
                LookAt(lookPosition, Vector3.Up);

				break;

			default:
				break;
		}

		animationTree.Set("parameters/conditions/idle", inRange);
		animationTree.Set("parameters/conditions/run", !inRange);
		
		MoveAndSlide();
    }

	private void playerDetected()
	{
		if (shootingRange.HasOverlappingAreas())
		{
			currentState = state.attack;
			animationPlayer.Play("attack", 0.2);
		}
		else
		{
			currentState = state.chase;
			animationPlayer.Play("run", 0.2);
		}
		
	}

	private void playerInRange()
	{
		if (currentState == state.chase)
		{
			currentState = state.attack;
			animationPlayer.Play("attack", 0.2);
		}
		
	}

	private void playerOutOfRange()
    {
		if (currentState == state.attack)
		{
			currentState = state.chase;
			animationPlayer.Play("run", 0.2);
		}
        
    }

	private void hit()
	{
		currentState = state.dead;
		skeleton.PhysicalBonesStartSimulation();
	}

	private void switchDestination()
	{
		if (nextLocation == locationA)
		{
			nextLocation = locationB;
		}
		else
		{
			nextLocation = locationA;
		}
	}

	private void patrolBreakTimeout()
	{
		animationPlayer.Play("walk", 0.2);
		currentState = state.patrol;
	}
}
