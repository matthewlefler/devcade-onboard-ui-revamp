using Godot;

public partial class NotificationWindow : Window
{
    public override void _EnterTree()
    {
        this.CloseRequested += this.QueueFree;
        
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        this.Position = screenSize - this.Size - new Vector2I((screenSize.X - this.Size.X) / 2, 0);

        this.AlwaysOnTop = true;
    }

    public override void _Ready()
    {
        this.Show();
        DisplayServer.WindowMoveToForeground(this.GetWindowId());
    }

    public override void _ExitTree()
    {
        // when killed kill the main onboard window/process too
        GetTree().Quit(0);
    }

    public void show()
    {
        this.Show();
        DisplayServer.WindowMoveToForeground(this.GetWindowId());
    }

    public void hide()
    {
        this.hide();
    }
}
