using Godot;

public partial class NotificationWindow : Window
{
    private Vector2I correct_position;

    public override void _EnterTree()
    {
        this.CloseRequested += this.QueueFree;
        
        Vector2I screenSize = DisplayServer.ScreenGetSize();
        correct_position = screenSize - this.Size - new Vector2I((screenSize.X - this.Size.X) / 2, 0);
        this.Position = correct_position;

        this.AlwaysOnTop = true;
    }

    private int id;
    public override void _Ready()
    {
        this.id = this.GetWindowId(); 
        this.show();
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
        DisplayServer.WindowMoveToForeground(id);
        this.GrabFocus();
    }

    public void hide()
    {
        if(!Visible) { return; }

        this.hide();
    }
}
