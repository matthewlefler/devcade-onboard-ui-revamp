using Godot;

namespace onboard.util; 

/// <summary>
/// 
/// </summary>
public static class Logger
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
    public static Log GetLogger(string className)
    {
        return new Log(className);
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

    public static string stringWrapLogLevelColor(string messageToWrap, Level logLevel)
    {
        switch(logLevel)
        {
            case Level.trace:
                return $"[color=GREEN]{messageToWrap}[/color]";
            case Level.verbose:
                return $"[color=OLD_LACE]{messageToWrap}[/color]";
            case Level.debug:
                return $"[color=PURPLE]{messageToWrap}[/color]";
            case Level.info:
                return $"[color=GREEN]{messageToWrap}[/color]";
            case Level.warn:
                return $"[color=YELLOW]{messageToWrap}[/color]";
            case Level.error:
                return $"[color=RED]{messageToWrap}[/color]";
            case Level.fatal:
                return $"[color=RED]{messageToWrap}[/color]";

            default:
                return $"[color=green]{messageToWrap}[/color]";
        }
    }
}