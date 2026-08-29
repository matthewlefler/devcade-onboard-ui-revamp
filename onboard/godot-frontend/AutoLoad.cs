using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using onboard.util;

namespace onboard; 

public partial class AutoLoad : Node
{
    util.Logger LOG = Log.get(nameof(AutoLoad));

    /// <summary>
    /// load the required services as early as possible
    /// with set properties
    /// </summary>
    public override void _Ready()
    {
        // load the .env file (contains the enviorment variables)
        LOG.Info("loading env");
        List<string> envPaths = [
            "~/.env",
            "./.env",
            "/usr/share/devcade/.env"
        ];
        foreach(string path in envPaths)
        {
            string fullPath = path
                .Replace("~/", $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)}/")
                .Replace("./", $"{Directory.GetCurrentDirectory()}/");

            if(File.Exists(fullPath))
            {
                LOG.Debug($"loaded enivorment file (.env) from {path} with full path {fullPath}");
                Env.load(fullPath);
            }
        }

        string logLocation = Env.LOG_LOCATION();
        try
        {
            string targetLogPath = ProjectSettings.GetSetting("debug/file_logging/log_path").AsString();
            string[] subStrings = targetLogPath.Split('/');
  
            targetLogPath = targetLogPath.Substring(0, targetLogPath.LastIndexOf(subStrings.Last())); // remove file ie "godot.log" from right side
            targetLogPath = ProjectSettings.GlobalizePath(targetLogPath);

            // remove link if target locations differ
            if(Directory.Exists(logLocation))
            {
                FileSystemInfo target = Directory.ResolveLinkTarget(logLocation, true);
                if(target.FullName != targetLogPath)
                {
                    LOG.Info("removing link with target: " + target.FullName);
                    Directory.Delete(logLocation);

                    Directory.CreateSymbolicLink(logLocation, targetLogPath);
                    LOG.Info("created symlink to: " + targetLogPath);
                }
            }
            else
            {
                Directory.CreateSymbolicLink(logLocation, targetLogPath);
                LOG.Info("created symlink to: " + targetLogPath);
            }

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
