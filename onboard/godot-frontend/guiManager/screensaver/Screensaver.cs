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

    Vector2 shaderVelInit;

    public void play()
    {
        playing = true;   
        currentGameAnimationIndex = 0;
        gamesAnimationsContainer.Position = startPosition;

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

        shaderVelInit = getShaderVel();
    }

    public override void _Process(double delta)
    {
        if(!playing)
        {
            setShaderVel(shaderVelInit);
            return;
        }

        setShaderVel(new Vector2(-scrollSpeed / 2.918f, shaderVelInit.Y));

        gamesAnimationsContainer.Position += new Vector2((float) delta * -scrollSpeed * 100.0f, 0);

        if(gamesAnimationsContainer.Position.X < endPosition.X)
        {
            gamesAnimationsContainer.Position = startPosition;
        }
    }

    private void setShaderVel(Vector2 v)
    {
        backgroundIcons.Material.Set("shader_parameter/direction", v);
    }

    private Vector2 getShaderVel()
    {
        return backgroundIcons.Material.Get("shader_parameter/direction").AsVector2();
    }
}
