using Godot;

// used for animations
public partial class GameButton
{
	public BaseButton childButton;
	// how close this button can be to the target rotation before it snaps to the target rotation
	const float errorMargin = 0.05f;
	public float targetRotation; 

	public bool isInsideTree {get { return childButton.IsInsideTree(); } private set {} }

	public GameButton(float targetRotation, BaseButton childButton)
	{
		this.targetRotation = targetRotation;
		this.childButton = childButton;
	}

	public void process(double delta)
	{

		if(!(targetRotation - this.childButton.Rotation < errorMargin && targetRotation - this.childButton.Rotation > -errorMargin))
		{
			this.childButton.Rotation += (targetRotation - this.childButton.Rotation) * 20f * (float) delta;
		}
		else
		{
			this.childButton.Rotation = targetRotation;
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
