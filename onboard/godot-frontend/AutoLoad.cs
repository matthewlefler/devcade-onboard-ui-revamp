using Godot;
using onboard.util;

namespace onboard; 

public partial class AutoLoad : Node
{
    public AutoLoad() {} // must be empty?

    /// <summary>
    /// load the config file as early as possible
    /// with set properties
    /// </summary>
    public override void _Ready()
    {
        // load the .env file (contains the enviorment variables)
        GD.Print("loading env");
        Env.load("../.env");

        // force initalization of:

        // start client (backend networked communicator)
        GD.Print("starting backend client");
        devcade.Client.init();
    }
}
