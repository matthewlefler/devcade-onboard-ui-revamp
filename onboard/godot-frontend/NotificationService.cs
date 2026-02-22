using System.Diagnostics;
using Godot;

public partial class NotificationService : Node
{
    private Process notificationService;
    public override void _Ready()
    {      
        string command = "~/.devcade/notification_service -t";
        var escapedArgs = command.Replace("\"", "\\\""); // Escape double quotes within the command (" become \")
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash", // Specify the bash executable
            Arguments = $"-c \"{escapedArgs}\"", // Pass the command using the -c option
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false, // Must be false to redirect streams
            CreateNoWindow = false

        };

        notificationService = new Process
        {
            StartInfo = startInfo
        };

        notificationService.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                GD.Print($"notification-service: {args.Data}");
        };

        notificationService.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                GD.PushError($"notification-service: {args.Data}");
        };

        bool started = notificationService.Start();
        notificationService.BeginOutputReadLine();
        notificationService.BeginErrorReadLine();

        if(!started)
        {
            GD.PushError("Failed to start Notification Service");
        }
        else
        {
            GD.Print("Suceeded in starting Notification Service");
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
