using Godot;

public partial class ScreenSaverGameAnimation : Control
{
    [Export]
    public string game_name = "Null";

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

    virtual public void play()
    {
        
    }

    virtual public void stop()
    {
        
    }
}
