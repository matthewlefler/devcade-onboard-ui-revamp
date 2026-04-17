using Godot;
using onboard.util;
using System.Diagnostics;

public partial class ScreenSaverGameAnimation : Control
{
    static bool lowPerformanceMode = false;

    [Export]
    public string game_name = "Null";

    [Export]
    public VideoStreamPlayer videoStreamPlayer;

    [Export]
    public TextureRect textureRect;


    public override void _Ready()
    {
        lowPerformanceMode = Env.LOW_PERFORMANCE_MODE();

        this.textureRect.Texture = this.videoStreamPlayer.GetVideoTexture();

        if(lowPerformanceMode)
        {
            this.videoStreamPlayer.Hide();
        }
        else
        {
            this.textureRect.Hide();
        }

        this.Hide();
    }

    public override void _Notification(int what)
    {
        if(what == NotificationVisibilityChanged)
        {
            if(!lowPerformanceMode && this.videoStreamPlayer.IsInsideTree())
            {
                if(videoStreamPlayer == null) { return; }
                if(this.Visible)
                {
                    this.play();
                }
                else
                {
                    this.stop();
                }
            }
        }
    }

    public void play()
    {
        if(lowPerformanceMode || videoStreamPlayer == null) { return; }

        videoStreamPlayer.Play();
        videoStreamPlayer.Paused = false;
    }

    public void stop()
    {
        if(videoStreamPlayer == null) { return; }

        videoStreamPlayer.Stop();
        videoStreamPlayer.Paused = true;
    }
}
