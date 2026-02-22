using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class NotificationService : Node
{
    private Process notificationService;
    public override void _Ready()
    {      
        bool started = "~/.devcade/notification_service -t".Bash();

        if(!started)
        {
            GD.PushError("Failed to start Notification Service");
        }
        else
        {
            GD.Print("Suceeded in starting Notification Service");

            "wmctrl -r notifications -b add,above".Bash();
            "wmctrl -r godot-frontend -b remove,above".Bash();
            "wmctrl -r godot-frontend -b add,below".Bash();
        }
    }

    public override void _Process(double delta)
    {
        
    }

    public override void _ExitTree()
    {
        if(notificationService != null)
        {
            notificationService.Kill();   
        }
    }
}

public static class ShellHelper
{
    public static bool Bash(this string cmd)
    {
        var source = new TaskCompletionSource<int>();
        var escapedArgs = cmd.Replace("\"", "\\\"");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },

            EnableRaisingEvents = true
        };
        
        process.Exited += (sender, args) =>
        {
            GD.PushWarning(process.StandardError.ReadToEnd());
            GD.PushError(process.StandardOutput.ReadToEnd());

            process.Dispose();
        };

        try
        {
            return process.Start();
        }
        catch (Exception e)
        {
            GD.PushError(e, "Command {} failed", cmd);
        }

        return false;
    }
}
