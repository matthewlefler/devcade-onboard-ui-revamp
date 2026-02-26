using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class NotificationService : Node
{
    // private Process notificationService;

    private Window notificationWindow;

    public override void _Ready()
    {      

    }

    public override void _Process(double delta)
    {
        
    }

    public override void _ExitTree()
    {

    }
}

public static class ShellHelper
{
    public static Process Bash(this string cmd)
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
            string stdErr = process.StandardError.ReadToEnd();
            if(stdErr.Length > 0)
            {
                GD.PushError(stdErr);
            }
            string stdOut = process.StandardOutput.ReadToEnd();
            if(stdOut.Length > 0)
            {
                GD.Print(stdOut);
            }

            process.Dispose();
        };

        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            GD.PushError(e, "Command {} failed", cmd);
            return null;
        }

        return process;
    }
}
