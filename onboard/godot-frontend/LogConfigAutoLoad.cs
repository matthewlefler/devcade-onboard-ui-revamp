using Godot;
using onboard.util;

namespace onboard; 

public partial class LogConfigAutoLoad : Node
{
    public LogConfigAutoLoad() {} // must be empty?

    /// <summary>
    /// load the config file as early as possible
    /// with set properties
    /// </summary>
    public override void _EnterTree()
    {
        // load the .env file (contains the enviorment variables)
        Env.load("../.env");

        // start client (backend networked communicator)
        devcade.Client.init();
    }
}
