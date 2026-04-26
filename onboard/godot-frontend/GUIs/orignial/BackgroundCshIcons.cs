using Godot;
using onboard.util;

public partial class BackgroundCshIcons : TextureRect
{
    public override void _Ready()
    {
        if(Env.LOW_PERFORMANCE_MODE())
        {
            this.Material = null;
        }
    }
}
