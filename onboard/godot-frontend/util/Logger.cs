using Godot;

namespace onboard.util; 

/// <summary>
/// 
/// </summary>
public static class Logger
{
    public static Log.Level currentLogLevel = Log.Level.debug;
    public static Log GetLogger(string prepend)
    {
        return new Log(prepend);
    }

    /// <summary>
    /// Logs (prints) an arbitrary message at a given log level
    /// </summary>
    /// <param name="msg"> the message to send </param>
    /// <param name="logLevel"> The level to log </param>
    public static void logMessage(string message, Log.Level logLevel)
    {   
        if(
            logLevel == Log.Level.warn ||
            logLevel == Log.Level.debug
        )
        {
            GD.PushWarning(message);
        }
        if(
            logLevel == Log.Level.error ||
            logLevel == Log.Level.fatal
        )
        {
            GD.PushError(message);
        }   

        GD.Print(message);
    }
}