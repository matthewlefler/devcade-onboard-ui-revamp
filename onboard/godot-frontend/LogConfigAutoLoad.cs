using Godot;
using onboard;
using onboard.util;
using System.IO;

public partial class LogConfigAutoLoad : Node
{
    /// <summary>
    /// load the config file as early as possible
    /// with set properties
    /// </summary>
    public LogConfigAutoLoad()
    {
        // load the .env file (contains the enviorment variables)
        Env.load("../.env");

        // log file name
        log4net.GlobalContext.Properties["LogFileName"] = "lastest.log";
        // set where to log
        log4net.GlobalContext.Properties["LogFilePath"] = Env.get("DEVCADE_PATH").unwrap_or("~/.devcade") + "/logs/frontend";
        // load the configuration file
        log4net.Config.XmlConfigurator.Configure(new FileInfo("app.config"));
        LogConfig.init(LogConfig.Level.DEBUG);
    }
}
