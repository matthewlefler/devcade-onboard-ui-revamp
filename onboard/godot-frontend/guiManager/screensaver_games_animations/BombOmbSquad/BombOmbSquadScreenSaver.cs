using Godot;

public partial class BombOmbSquadScreenSaver : ScreenSaverGameAnimation
{
    [Export]
    public VideoStreamPlayer videoStreamPlayer;

    public override void play()
    {
        videoStreamPlayer.Play();
    }

    public override void stop()
    {
        videoStreamPlayer.Stop();
    }
}
