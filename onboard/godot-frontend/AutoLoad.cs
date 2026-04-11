using System;
using System.IO;
using Godot;
using onboard.util;

namespace onboard; 

public partial class AutoLoad : Node
{
    Logger LOG = Log.get(nameof(AutoLoad));

    public AutoLoad() {} // must be empty?

    /// <summary>
    /// load the config file as early as possible
    /// with set properties
    /// </summary>
    public override void _Ready()
    {
        // load the .env file (contains the enviorment variables)
        LOG.Info("loading env");
        Env.load("../.env");

        string logLocation = Env.LOG_LOCATION();
        if(!Path.Exists(logLocation))
        {
            try
            {
                Directory.CreateDirectory(logLocation);
            }
            catch (Exception e)
            {
                LOG.Error(e.Message);
                logLocation = "user://logs/godot.log"; // default godot log location, use if other log location fails
            }
        }
        ProjectSettings.SetSetting("debug/file_logging/log_path", logLocation);

        // force initalization of:

        // start client (backend networked communicator)
        LOG.Info("starting backend client");
        devcade.Client.init();
    }
}
