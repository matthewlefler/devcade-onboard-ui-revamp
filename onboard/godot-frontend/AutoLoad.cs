using System;
using System.IO;
using System.Linq;
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
        try
        {
            string targetLogPath = ProjectSettings.GetSetting("debug/file_logging/log_path").AsString();
            string[] subStrings = targetLogPath.Split('/');
            foreach (string item in subStrings)
            {
                LOG.Error(item);
            }
  
            targetLogPath = targetLogPath.Substring(0, targetLogPath.LastIndexOf(subStrings.Last())); // remove file ie "godot.log" from right side
            targetLogPath = ProjectSettings.GlobalizePath(targetLogPath);

            // remove link if target locations differ
            if(File.Exists(logLocation))
            {
                FileInfo linkInfo = new FileInfo(logLocation);
                FileSystemInfo target = linkInfo.ResolveLinkTarget(returnFinalTarget: true);
                if(target.FullName != targetLogPath)
                {
                    LOG.Info("removing link with target: " + target.FullName);
                    File.Delete(logLocation);
                }
            }

            Directory.CreateSymbolicLink(logLocation, targetLogPath);
            LOG.Info("created symlink to: " + targetLogPath);
        }
        catch (Exception e)
        {
            LOG.Error("Unable to create symlink: " + e.Message);
        }

        // force initalization of:

        // start client (backend networked communicator)
        LOG.Info("starting backend client interface ");
        devcade.Client.init();
    }
}
