using Godot;
using System;
using System.Threading.Tasks;


public partial class crosshair : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		await ScaleCrosshair();
		
	}

	public async Task ScaleCrosshair()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		Scale = new Vector2(4,4);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
