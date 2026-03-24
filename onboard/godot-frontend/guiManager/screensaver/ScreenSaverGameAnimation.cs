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
        if(what == NotificationVisibilityChanged && this.IsInsideTree())
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
        videoStreamPlayer.Play();
        videoStreamPlayer.Paused = false;
    }

    public void stop()
    {
        videoStreamPlayer.Stop();
        videoStreamPlayer.Paused = true;
    }
}
