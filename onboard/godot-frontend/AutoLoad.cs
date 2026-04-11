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
    /// load the required services as early as possible
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
                string logPath = ProjectSettings.GetSetting("debug/file_logging/log_path").AsString();
                Directory.CreateSymbolicLink(logLocation, ProjectSettings.GlobalizePath(logPath));
                LOG.Info("created symlink to: " + logPath);
            }
            catch (Exception e)
            {
                LOG.Error("Unable to create symlink: " + e.Message);
            }
        }

        // force initalization of:

        // start client (backend networked communicator)
        LOG.Info("starting backend client interface ");
        devcade.Client.init();
    }
}
