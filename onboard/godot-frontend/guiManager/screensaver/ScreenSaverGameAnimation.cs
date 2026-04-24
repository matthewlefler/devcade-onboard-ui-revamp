using Godot;

public partial class ScreenSaverGameAnimation : Control
{
    [Export]
    public string game_name = "Null";

    [Export]
    public VideoStreamPlayer videoStreamPlayer;

    private float screenWidth = 0;

    public override void _Ready()
    {
        this.Hide();
        this.screenWidth = GetViewport().GetVisibleRect().Size.X;
        GetViewport().SizeChanged += () => {
            this.screenWidth = GetViewport().GetVisibleRect().Size.X;
        };
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

    public override void _Process(double delta)
    {
        float xPosition = this.GlobalPosition.X;
        if(xPosition > -screenWidth && xPosition < screenWidth)
        {
            this.play();
        }
        else
        {
            this.stop();
        }
    }

    public void play()
    {
        if(videoStreamPlayer == null) { return; }

        videoStreamPlayer.Paused = false;
    }

    public void stop()
    {
        if(videoStreamPlayer == null) { return; }

        videoStreamPlayer.Paused = true;
    }
}
