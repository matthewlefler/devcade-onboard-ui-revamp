using Godot;
using System;

public partial class Background : TextureRect
{
    /// <summary>
    /// camera to get the viewport of
    /// </summary>
    [Export]
    Camera2D camera;

    public override void _Ready()
    {
        this.CustomMinimumSize = camera.GetViewportRect().Size;

        base._Ready();
    }
}
