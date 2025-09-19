using Godot;

namespace onboard.devcade.GUI.originalGUI;

public partial class SlerpCamera2d : Camera2D
{
    /// <summary>
    /// the number of valid positions for the camera left of its initial position
    /// </summary>
    [Export]
    public int positionsLeft = 1;

    /// <summary>
    /// the number of valid positions for the camera right of its initial position
    /// </summary>
    [Export]
    public int positionsRight = 1;

    /// <summary>
    /// the amount to ease the animation by from 0.0 to 1.0f
    /// </summary>
    [Export]
    public float easeAmount = 1;

    /// <summary>
    /// the scale of the speed of the animation from 1.0f to inf.
    /// </summary>
    [Export]
    public float animationSpeed = 2.0f;
    private Vector2[] positions;

    private int targetIndex;
    private int previousTargetIndex;

    private float time = 0;

    public override void _Ready()
    {
        calculatePositions(this.GetViewportRect().Size.X);

        base._Ready();
    }

    private void calculatePositions(float viewportWidth)
    {
        positions = new Vector2[positionsLeft + positionsRight + 1];
        
        for (int i = positionsLeft; i >= 0; i--)
        {
            positions[i] = new Vector2(viewportWidth * -i, this.Position.Y); 
        }

        positions[positionsLeft] = this.Position;

        for (int i = 1; i <= positionsRight; i++)
        {
            positions[positionsLeft + i] = new Vector2(viewportWidth * i, this.Position.Y); 
        }

        targetIndex = positionsLeft;
        previousTargetIndex = positionsLeft;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        time += (float)delta * animationSpeed;

        if (time > 1)
        {
            time = 1;
        }

        Vector2 startPosition = positions[previousTargetIndex];
        Vector2 endPosition = positions[targetIndex];

        Vector2 offset = endPosition - startPosition;

        this.Position = CubicBezier(startPosition, startPosition + (offset / easeAmount), endPosition - (offset / easeAmount), endPosition, time);
    }

    public void moveRight()
    {
        previousTargetIndex = targetIndex;

        targetIndex++;

        if (targetIndex > positions.Length - 1)
        {
            targetIndex = positions.Length - 1;
        }

        time = 0;
    }

    public void moveLeft()
    {
        previousTargetIndex = targetIndex;

        targetIndex--;

        if (targetIndex < 0)
        {
            targetIndex = 0;
        }

        time = 0;
    }

    public void setTargetIndex(int index)
    {
        if (index > positions.Length - 1)
        {
            index = positions.Length - 1;
        }
        if (index < 0)
        {
            index = 0;
        }

        previousTargetIndex = targetIndex;
        targetIndex = index;

        time = 0;
    }
    
    private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 q0 = p0.Lerp(p1, t);
        Vector2 q1 = p1.Lerp(p2, t);
        Vector2 q2 = p2.Lerp(p3, t);

        Vector2 r0 = q0.Lerp(q1, t);
        Vector2 r1 = q1.Lerp(q2, t);

        Vector2 s = r0.Lerp(r1, t);
        return s;
    }
}
