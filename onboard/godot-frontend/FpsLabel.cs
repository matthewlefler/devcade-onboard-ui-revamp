using Godot;
using onboard.devcade;
using System;
using System.Linq;

public partial class FpsLabel : Label
{

    int[] fpsSave = new int[100];
    int lowFps;
    int i = 0;

    public override void _Process(double delta)
    {
        if(Client.isProduction)
        {
            this.Hide();
            return;
        }
        this.Show();

        int fps = (int) (1.0 / delta);
        fpsSave[i] = fps;
        i = (i + 1) % fpsSave.Length;

        fps = (int) fpsSave.Average();
        lowFps = fpsSave.Min();

        this.Text = 
            "avg: " + fps.ToString() + "\n" +
            "low: " + lowFps.ToString();

        Color color;
        if(fps < 30)
        {
            color = Colors.Red;
        }
        else if( fps < 60 )
        {
            color = Colors.Yellow;
        }
        else
        {
            color = Colors.Green;
        }

        this.Set("theme_override_colors/font_color", color);
    }
}
