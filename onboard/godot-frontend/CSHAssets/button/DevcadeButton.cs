using Godot;

public partial class DevcadeButton : AnimatedSprite2D
{
    public override void _Notification(int what)
    {
        if(what == NotificationVisibilityChanged)
        {
            if(this.Visible)
            {
                this.Play();
            }
            else
            {
                this.Stop();
            }
        }
    }
}
