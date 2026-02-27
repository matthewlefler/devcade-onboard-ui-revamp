using System;
using System.Collections.Generic;
using Godot;

public partial class Screensaver : Control
{
    [Export]
    public float scrollSpeed = 1.0f;

    [Export]
    private Control gamesAnimationsContainer;

    [Export]
    private TextureRect backgroundIcons;
    
    private List<ScreenSaverGameAnimation> gameAnimationNodes = new List<ScreenSaverGameAnimation>();

    private bool playing = false;
    private int currentGameAnimationIndex = 0;

    Vector2 startPosition;
    Vector2 endPosition;
    float screenWidth;

    public void play()
    {
        playing = true;   
        currentGameAnimationIndex = 0;
        gamesAnimationsContainer.Position = startPosition;
        shader_offset = 0.0f;

        foreach(var anim in gameAnimationNodes)
        {
            anim.Show();
        }
    }

    public void stop()
    {
        playing = false;

        foreach(var anim in gameAnimationNodes)
        {
            anim.Hide();
        }
    }

    public override void _Ready()
    {
        screenWidth = GetViewportRect().Size.X;

        startPosition = new Vector2(0, 0);

        foreach(Node node in gamesAnimationsContainer.GetChildren())
        {
            if(node is ScreenSaverGameAnimation gameAnimation)
            {
                gameAnimationNodes.Add(gameAnimation);
                gameAnimation.Position = startPosition;
            }
            else
            {
                GD.PrintErr($"found child node {node.Name} that is not a ScreenSaverGameAnimation");
            }
        }

        endPosition = new Vector2(-1 * screenWidth * (gameAnimationNodes.Count - 1), 0);

        for (int i = 0; i < gameAnimationNodes.Count; i++)
        {
            ScreenSaverGameAnimation anim = gameAnimationNodes[i];

            anim.Position = new Vector2(screenWidth * i, 0);
        }
    }

    float shader_offset = 0.0f;
    public override void _Process(double delta)
    {
        if(!playing)
        {
            return;
        }

        shader_offset += scrollSpeed / 500.0f;
        if(shader_offset > 1)
        {
            shader_offset -= 1;
        }

        setShaderXOffest(shader_offset);

        gamesAnimationsContainer.Position += new Vector2(-scrollSpeed, 0);

        if(gamesAnimationsContainer.Position.X < endPosition.X)
        {
            gamesAnimationsContainer.Position = startPosition;
        }
    }

    private void setShaderXOffest(float x)
    {
        backgroundIcons.Material.Set("shader_parameter/x_offset", x);
    }
}
