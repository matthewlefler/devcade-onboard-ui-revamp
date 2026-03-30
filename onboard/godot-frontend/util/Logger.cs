using Godot;

namespace onboard.util; 

/// <summary>
/// 
/// </summary>
public static class Log
{
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

    public static Level currentLogLevel = Level.debug;
    public static Logger GetLogger(string className)
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