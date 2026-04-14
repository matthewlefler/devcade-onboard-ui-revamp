using System.ComponentModel.DataAnnotations;
using Godot;

namespace onboard.util; 

/// <summary>
/// 
/// </summary>
public static class Log
{
    private static Level logLevel;

    public enum Level
    {
        trace,
        verbose,
        debug,
        info,
        warn,
        error,
        fatal,
    }

    static Log()
    {
        string level = Env.FRONTEND_LOG();

        logLevel = Level.error;
        if(level == "trace")   { logLevel = Level.trace; }
        if(level == "verbose") { logLevel = Level.verbose; }
        if(level == "debug")   { logLevel = Level.debug; }
        if(level == "info")    { logLevel = Level.info; }
        if(level == "warn")    { logLevel = Level.warn; }
        if(level == "error")   { logLevel = Level.error; }
        if(level == "fatal")   { logLevel = Level.fatal; }

        string time = Time.GetTimeStringFromSystem();
        logMessage($"[{time} INFO Log] Set current Log level to {logLevel}", Level.info);
    }

    public static Level currentLogLevel = Level.debug;
    public static Logger get(string className)
    {
        return new Logger(className);
    }

    /// <summary>
    /// Logs (prints) an arbitrary message at a given log level
    /// </summary>
    /// <param name="msg"> the message to send </param>
    /// <param name="logLevel"> The level to log </param>
    public static void logMessage(string message, Level logLevel)
    {   
        if(logLevel < Log.logLevel)
        {
            return;
        }
        
        if(
            logLevel == Level.warn ||
            logLevel == Level.debug
        )
        {
            GD.PushWarning(message);
        }
        if(
            logLevel == Level.error ||
            logLevel == Level.fatal
        )
        {
            GD.PushError(message);
        }   

        GD.Print(message);
    }
}