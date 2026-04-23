using Godot;

// used for animations
public class GameButton
{
	[Export]
	public float minimumRotationSpeed = 3.0f;
	[Export]
	public float maxRotationSpeed = 20.0f;

	public BaseButton childButton;

	/// <summary>
	/// how close this button can be to the target rotation before it snaps to the target rotation
	/// </summary>
	const float errorMargin = 0.05f;
	public float targetRotation; 

	public bool isInsideTree {get { return childButton.IsInsideTree(); } private set {} }

	public int index = -1;

	public GameButton(float targetRotation, BaseButton childButton, int index)
	{
		this.targetRotation = targetRotation;
		this.childButton = childButton;
		this.index = index;
	}

	public void process(double delta)
	{
		float deltaRotation = targetRotation - this.childButton.Rotation;
		float direction = float.Sign(deltaRotation);

		float d = (float) (1.0 + -1.0 / (1.0 + 0.13 * float.Abs(deltaRotation)));
		float rotationSpeed = float.Lerp(minimumRotationSpeed, maxRotationSpeed, d);

		if(deltaRotation < errorMargin && deltaRotation > -errorMargin)
		{
			this.childButton.Rotation = targetRotation;
		}
		else
		{
			this.childButton.Rotation += (float) (direction * rotationSpeed * delta);
		}

		if(this.childButton.Rotation > 3.2f || this.childButton.Rotation < -3.2f) 
		{
			this.childButton.Hide();
		}
		else 
		{
			this.childButton.Show();
		}
	}
}
