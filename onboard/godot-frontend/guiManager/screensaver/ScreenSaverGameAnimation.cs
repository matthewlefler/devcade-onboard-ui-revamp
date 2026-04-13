using Godot;

public partial class ScreenSaverGameAnimation : Control
{
    [Export]
    public string game_name = "Null";

    [Export]
    public VideoStreamPlayer videoStreamPlayer;

    public override void _Ready()
    {
        this.Hide();
    }

    public override void _Notification(int what)
    {
        if(videoStreamPlayer == null) { return; }

        if(what == NotificationVisibilityChanged && this.videoStreamPlayer.IsInsideTree())
        {
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

    public void play()
    {
        if(videoStreamPlayer == null) { return; }

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
