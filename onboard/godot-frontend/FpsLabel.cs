using Godot;
using onboard.devcade;
using System;

public partial class FpsLabel : Label
{
    public override void _Process(double delta)
    {
        if(Client.isProduction)
        {
            this.Hide();
            return;
        }
        this.Show();

        int fps = (int) (1.0 / delta);
        this.Text = "FPS: " + fps.ToString();

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
