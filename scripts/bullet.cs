using Godot;
using System;

public partial class bullet : Node3D
{
	public bool shoot = false;

	public const float speed = 60.0f;
	public const float damage = 10.0f;

	Vector3 zeroVector = new Vector3(0, 0, -speed);

	private Area3D area3D { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TopLevel = true;

		area3D = (Area3D)GetNode("Area3D");
		area3D.AreaEntered += (area) => _On_Area_body_Entered(area);
		area3D.BodyEntered += (body) => _On_Physics_body_Entered(body);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (shoot == true)
		{
			//ApplyImpulse(Transform.Basis.Z, -Transform.Basis.Z * speed);
			//ApplyCentralImpulse(-Transform.Basis.Z * speed);
			
		}
	}

    public override void _Process(double delta)
	{
		if (shoot == true)
		{
			Position += Transform.Basis * zeroVector * (float)delta;
		}
        
    }

    private void _On_Physics_body_Entered(Node body)
	{
		GD.Print("hit here");
		QueueFree();
	}

	private void _On_Area_body_Entered(Area3D area)
	{
		GD.Print("hit something");
		if (area.IsInGroup("Enemy"))
		{
			GD.Print("HIT");
			QueueFree();
		}
		else
		{
			QueueFree();
		}
	}
}
