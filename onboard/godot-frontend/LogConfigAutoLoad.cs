using Godot;
using onboard;
using System;
using System.IO;

public partial class LogConfigAutoLoad : Node
{
    /// <summary>
    /// load the config file as early as possible
    /// with set properties
    /// </summary>
    public LogConfigAutoLoad()
    {
        // log file name
        log4net.GlobalContext.Properties["LogFileName"] = "latest.log";
        // set where to log
        log4net.GlobalContext.Properties["LogFilePath"] = "/home/skye/.devcade/logs/frontend";
        log4net.Config.XmlConfigurator.Configure(new FileInfo("app.config"));
        LogConfig.init(LogConfig.Level.DEBUG);
    }
}
