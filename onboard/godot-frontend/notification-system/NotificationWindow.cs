using Godot;

public partial class NotificationWindow : Window
{
    public override void _EnterTree()
    {
        this.CloseRequested += this.QueueFree;
    }
}
