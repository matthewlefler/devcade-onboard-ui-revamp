using System;
using System.Collections.Generic;
using Godot;
using onboard;
using onboard.devcade;

public partial class Screensaver : Control
{
    [Export]
    public float scrollSpeed = 1.0f;

    [Export]
    private Control gamesAnimationsContainer;

    [Export]
    private TextureRect backgroundIcons;
    
    private List<ScreenSaverGameAnimation> shownGameAnimationNodes = new List<ScreenSaverGameAnimation>();
    private List<ScreenSaverGameAnimation> gameAnimationNodes = new List<ScreenSaverGameAnimation>();

    private bool playing = false;
    private int currentGameAnimationIndex = 0;

    private readonly Vector2 startPosition = new Vector2(0, 0);
    private Vector2 endPosition;
    float screenWidth;

    Vector2 shaderVelInit;

    public void play()
    {
        playing = true;   
        currentGameAnimationIndex = 0;
        gamesAnimationsContainer.Position = startPosition;

        foreach(var anim in shownGameAnimationNodes)
        {
            anim.Show();
        }
    }

    public void stop()
    {
        playing = false;

        foreach(var anim in shownGameAnimationNodes)
        {
            anim.Hide();
        }
    }

    public override void _Ready()
    {
        screenWidth = GetViewportRect().Size.X;

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

        shaderVelInit = getShaderVel();

        setScreenSaversShown(GuiManagerGlobal.gameTitles);

        GuiManagerGlobal.instance.gameTitlesUpdated += () =>
        {
            setScreenSaversShown(GuiManagerGlobal.gameTitles);
        };
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

    private void setScreenSaversShown(List<DevcadeGame> games)
    {
        if(games == null) { return; }

        shownGameAnimationNodes.Clear();

        foreach(ScreenSaverGameAnimation anim in gameAnimationNodes)
        {
            if(anim.game_name == "Background" || anim.game_name == "Background2")
            {
                shownGameAnimationNodes.Add(anim);
                continue;
            }

            foreach(DevcadeGame game in games)
            {
                string gameName = game.name;
                if(anim.game_name == gameName)
                {
                    GD.Print($"found matching anim: {anim.game_name}");
                    shownGameAnimationNodes.Add(anim);
                }
            }
        }

        endPosition = new Vector2(-1 * screenWidth * (shownGameAnimationNodes.Count - 1), 0);

        for (int i = 0; i < shownGameAnimationNodes.Count; i++)
        {
            ScreenSaverGameAnimation anim = shownGameAnimationNodes[i];
            GD.PrintErr($"anim: {anim.game_name} in shown");

            anim.Position = new Vector2(screenWidth * i, 0);
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
