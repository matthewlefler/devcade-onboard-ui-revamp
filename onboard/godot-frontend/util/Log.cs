using Godot;

namespace onboard.util; 

public class Log
{
    private string className = "null";

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

    public Log(string className)
    {
        this.className = className;
    }

    /// <summary>
    /// Logs (prints) an arbitrary message at a given log level
    /// </summary>
    /// <param name="msg"> the message to send </param>
    /// <param name="logLevel"> The level to log </param>
    public void logMessage(string msg, Level logLevel)
    {
        string time = Time.GetTimeStringFromSystem();

        string message = $"[{time} {stringWrapLogLevelColor(logLevel.ToString().ToUpperInvariant(), logLevel)} {className}] {msg}";

        Logger.logMessage(message, logLevel);
    }

    // trace, verbose, debug, info, warn, error, fatal methods
    
    public void Trace(string msg)
    {
        logMessage(msg, Level.trace);
    }

    public void Verbose(string msg)
    {
        logMessage(msg, Level.verbose);
    }

    public void Debug(string msg)
    {
        logMessage(msg, Level.debug);
    }

    public void Info(string msg)
    {
        logMessage(msg, Level.info);
    }

    public void Warn(string msg)
    {
        logMessage(msg, Level.warn);
    }

    public void Error(string msg)
    {
        logMessage(msg, Level.error);
    }

    public void Fatal(string msg)
    {
        logMessage(msg, Level.fatal);
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