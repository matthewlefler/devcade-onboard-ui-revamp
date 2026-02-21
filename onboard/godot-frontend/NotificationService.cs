using Godot;

public partial class NotificationService : Node
{
    [Export]
    private string NotificationServicePath = "/home/skye/gitrepos/devcade-onboard-ui-revamp/onboard/notifications/project.godot";
    private static readonly int failedToStart = -1;
    private int proccesId;
    public override void _EnterTree()
    {
        

        string[] args = {};

        proccesId = OS.CreateProcess($"godot-mono -t -w --path {NotificationServicePath}", args, openConsole: false);

        if(proccesId == failedToStart)
        {
            GD.PrintErr("Failed to start Notification Service");
            return;
        }
    }

    public override void _ExitTree()
    {
        OS.Kill(proccesId);
    }
}
