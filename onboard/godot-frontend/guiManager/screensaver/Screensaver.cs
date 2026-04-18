using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Godot;
using onboard;
using onboard.devcade;
using onboard.util;

public partial class Screensaver : Control
{
    private readonly Logger LOG = Log.get(nameof(Screensaver));

    [Export]
    public float scrollSpeed = 1.0f;

    [Export]
    private Control gamesAnimationsContainer;

    [Export]
    private PackedScene screensaverTemplate;

    private List<ScreensaverTemplate> shownGameAnimationNodes = new List<ScreensaverTemplate>();
    private List<ScreensaverTemplate> gameAnimationNodes = new List<ScreensaverTemplate>();

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
            anim.videoPlayer.Paused = false;
        }
    }

    public void stop()
    {
        playing = false;

        foreach(var anim in shownGameAnimationNodes)
        {
            anim.Hide();
            anim.videoPlayer.Paused = true;
        }
    }

    public override void _Ready()
    {
        if(screensaverTemplate == null)
        {
            LOG.Error("screensaverTemplate is not set");

            throw new ApplicationException("screensaverTemplate unset");
        }

        screenWidth = GetViewportRect().Size.X;

        create_screensavers();

        setScreenSaversShown(GuiManagerGlobal.gameTitles);

        GuiManagerGlobal.instance.gameTitlesUpdated += () =>
        {
            setScreenSaversShown(GuiManagerGlobal.gameTitles);
        };

        stop();
    }

    private void create_screensavers()
    {
        // download videos from backend
        GuiManagerGlobal.instance.downloadVideos().ContinueWith(_ =>
        {
            foreach (DevcadeGame game in GuiManagerGlobal.gameTitles)
            {
                string videoPath = $"{Env.DEVCADE_PATH()}/{game.id}/video.ogv";
                if(!File.Exists(videoPath))
                {
                    continue;
                }
            
                try 
                {
                    // godot image class 
                    VideoStream video = GD.Load<VideoStream>(videoPath);

                    ScreensaverTemplate node = screensaverTemplate.Instantiate<ScreensaverTemplate>();
                    node.videoPlayer.Stream = video;
                    node.gameId = game.id;
                    node.ZIndex = 4000;

                    gamesAnimationsContainer.AddChild(node);
                    gameAnimationNodes.Add(node);
                }
                catch (Exception e) {
                    LOG.Warn($"Unable to load video: {e.Message}");
                }
            }
        });
    }

    public override void _Process(double delta)
    {
        if(!playing)
        {
            return;
        }

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

        foreach(ScreensaverTemplate anim in gameAnimationNodes)
        {
            foreach(DevcadeGame game in games)
            {
                string gameId = game.id;
                // GD.Print($"found game: {game.name}");

                if(anim.gameId == gameId)
                {
                    // name instead of id b/c readability
                    GD.Print($"found matching anim: {game.name}");
                    shownGameAnimationNodes.Add(anim);
                }
            }
        }

        endPosition = new Vector2(-1 * screenWidth * (shownGameAnimationNodes.Count - 1), 0);

        for (int i = 0; i < shownGameAnimationNodes.Count; i++)
        {
            ScreensaverTemplate anim = shownGameAnimationNodes[i];

            anim.Position = new Vector2(screenWidth * i, 0);
        }
    }
}
