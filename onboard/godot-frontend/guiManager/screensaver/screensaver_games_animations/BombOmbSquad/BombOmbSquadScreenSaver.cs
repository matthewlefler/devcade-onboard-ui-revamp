using Godot;

public partial class BombOmbSquadScreenSaver : ScreenSaverGameAnimation
{
    [Export]
    public VideoStreamPlayer videoStreamPlayer;

    public override void play()
    {
        videoStreamPlayer.Play();
        videoStreamPlayer.Paused = false;
    }

    public override void stop()
    {
        videoStreamPlayer.Stop();
        videoStreamPlayer.Paused = true;
    }
}
