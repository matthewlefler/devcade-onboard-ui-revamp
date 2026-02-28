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

    private int id;
    public override void _Ready()
    {
        this.id = this.GetWindowId(); 
        this.Show();
        DisplayServer.WindowMoveToForeground(id);
    }

    public override void _ExitTree()
    {
        // when killed kill the main onboard window/process too
        GetTree().Quit(0);
    }

    public void show()
    {
        if(Visible) { return; }

        this.Show();
        // DisplayServer.WindowMoveToForeground(id);
    }

    public void hide()
    {
        if(!Visible) { return; }

        this.hide();
    }
}
