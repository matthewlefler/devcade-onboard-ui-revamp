using Godot;

public partial class FlashingRichTextLabel : RichTextLabel
{
    [Export]
    Color start_color = Colors.White;
    [Export]
    Color end_color = Colors.Black;

    [Export]
    double animation_speed = 1.0;

    public override void _Ready()
    {
        this.Set("theme_override_colors/default_color", start_color);
    }

    private bool color_dir = true;
    private double t = 0.0;
    public override void _Process(double delta)
    {
        t += delta;

        if(t > 1.0)
        {
            t = 0.0;
            color_dir = !color_dir;
        }

        if(color_dir)
        {
            set_font_color(start_color.Lerp(end_color, (float) t));
        }
        else
        {
            set_font_color(end_color.Lerp(start_color, (float) t));
        }
    }

    private void set_font_color(Color color)
    {
        this.Set("theme_override_colors/default_color", color);
        // this.AddThemeColorOverride("theme_override_colors/default_color", color);
    }
}
