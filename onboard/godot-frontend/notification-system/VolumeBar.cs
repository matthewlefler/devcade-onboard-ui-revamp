using System.Diagnostics;
using Godot;

public partial class VolumeBar : ProgressBar
{
    Process process = new Process {
        StartInfo = new ProcessStartInfo
        {
            FileName = "pamixer",
            Arguments = $"--get-volume",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        },
        EnableRaisingEvents = true
    };

    public override void _Process(double delta)
    {
        this.Value = (float) getVolume() / 100.0f;
    }

    private int getVolume()
    {
        bool started = process.Start();

        if(!started)
        {
            GD.PushWarning("command not run");
            return -1;
        }

        process.WaitForExit();
        
        string stdout = process.StandardOutput.ReadToEnd();
        
        return stdout.ToInt();
    }
}
