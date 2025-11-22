using Godot;
using log4net;
using onboard.util;
using System.IO;

namespace onboard; 

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
        log4net.GlobalContext.Properties["LogFileName"] = ".log";
        // set where to log
        log4net.GlobalContext.Properties["LogFilePath"] = Env.get("DEVCADE_PATH").unwrap_or("~/.devcade") + "/logs/frontend";
        // load the configuration file
        log4net.Config.XmlConfigurator.Configure(new FileInfo("app.config"));

        Option<string> levelOption = Env.get("FRONTEND_LOG");

        LogConfig.Level level;
        // Allowed log levels: trace, verbose, debug, info, warn, error, fatal 
        switch (levelOption.unwrap_or("none"))
        {
            case "trace":
                level = LogConfig.Level.TRACE;
                break;
            case "verbose":
                level = LogConfig.Level.VERBOSE;
                break;
            case "debug":
                level = LogConfig.Level.DEBUG;
                break;
            case "info":
                level = LogConfig.Level.INFO;
                break;
            case "warn":
                level = LogConfig.Level.WARN;
                break;
            case "error":
                level = LogConfig.Level.ERROR;
                break;
            case "fatal":
                level = LogConfig.Level.FATAL;
                break;
            default:
                level = LogConfig.Level.FATAL;
                break;
        }

        LogConfig.init(level);

        ILog logger = LogManager.GetLogger("onboard");
        logger.Info(Time.GetDateStringFromSystem());
        
        if (levelOption.is_none())
        {
            logger.Error("FRONTEND_LOG is not set to a valid value");
        }
    }
}
