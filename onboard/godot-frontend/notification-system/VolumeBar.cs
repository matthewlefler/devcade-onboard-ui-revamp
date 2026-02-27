using System.Diagnostics;
using Godot;

public partial class VolumeBar : ProgressBar
{
    [Export]
    NotificationWindow window;

    private Process process = new Process {
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

    private int last_volume = -1;

    [Export]
    double secondsBetweenPolls = 0.2;
    private double seconds = 0.0;
    
    [Export]
    double lingerTime = 1.0;
    private double lingerSec;
    public override void _Ready()
    {
        lingerSec = lingerTime;

        this.Visible = false;

        last_volume = getVolume();
    }

    public override void _Process(double delta)
    {
        seconds += delta;
        lingerSec += delta;

        if(lingerSec >= lingerTime)
        {
            this.Visible = false;
            lingerSec = lingerTime;
        }
        
        if(seconds < secondsBetweenPolls)
        {
            return;
        }
        seconds = 0.0;

        int volume = getVolume();

        if(volume != last_volume)
        {
            this.Visible = true;
            lingerSec = 0.0;
            window.show();
        }

        last_volume = volume;

        this.Value = getVolume() / 100.0f;
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
