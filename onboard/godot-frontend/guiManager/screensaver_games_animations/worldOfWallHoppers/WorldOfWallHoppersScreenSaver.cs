using Godot;

public partial class WorldOfWallHoppersScreenSaver : ScreenSaverGameAnimation
{
    [Export]
    public VideoStreamPlayer videoStreamPlayer;

    override public void play()
    {
        videoStreamPlayer.Play();
    }

    public override void stop()
    {
        videoStreamPlayer.Stop();
    }
}
