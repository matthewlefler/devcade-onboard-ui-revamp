using Godot;

public partial class NotificationWindow : Window
{
    public override void _EnterTree()
    {
        this.CloseRequested += this.QueueFree;
        
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        this.Position = screenSize - this.Size - new Vector2I((screenSize.X - this.Size.X) / 2, 0);

        this.AlwaysOnTop = true;
        this.Transient = false;
    }

    public override void _Ready()
    {
        this.Show();
        this.GrabFocus();
    }
}
