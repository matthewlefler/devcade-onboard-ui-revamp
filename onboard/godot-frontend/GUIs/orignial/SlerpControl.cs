using Godot;
using System;

public partial class SlerpControl : Control
{
	public Vector2 targetPosition = new Vector2(DisplayServer.ScreenGetSize().X, 0.0f);
	private Vector2 direction = new Vector2(0.0f, 0.0f);
	private float velocity = 1.0f;

	[Export]
	public float speed = 10f;

	public override void _Process(double delta)
	{
		if(Position.DistanceTo(targetPosition) < 2.0f)
		{
			// If the distance to the target position is less than 0.1, snap to the target position
			Position = targetPosition;
		}
		else
		{
			Position = Position.Slerp(targetPosition, speed * (float) delta);
		}
	}
}
