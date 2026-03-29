using System.Dynamic;
using Godot;

namespace onboard.util; 

public class Log
{
    public string className {get; private set;} = "null";

    public Log(string className)
    {
        this.className = className;
    }

    /// <summary>
    /// Logs (prints) an arbitrary message at a given log level
    /// </summary>
    /// <param name="msg"> the message to send </param>
    /// <param name="logLevel"> The level to log </param>
    public void logMessage(string msg, Logger.Level logLevel)
    {
        string time = Time.GetTimeStringFromSystem();

        string message = $"[{time} {Logger.stringWrapLogLevelColor(logLevel.ToString().ToUpperInvariant(), logLevel)} {className}] {msg}";

        Logger.logMessage(message, logLevel);
    }

    // trace, verbose, debug, info, warn, error, fatal methods
    
    public void Trace(string msg)
    {
        logMessage(msg, Logger.Level.trace);
    }

    public void Verbose(string msg)
    {
        logMessage(msg, Logger.Level.verbose);
    }

    public void Debug(string msg)
    {
        logMessage(msg, Logger.Level.debug);
    }

    public void Info(string msg)
    {
        logMessage(msg, Logger.Level.info);
    }

    public void Warn(string msg)
    {
        logMessage(msg, Logger.Level.warn);
    }

    public void Error(string msg)
    {
        logMessage(msg, Logger.Level.error);
    }

    public void Fatal(string msg)
    {
        logMessage(msg, Logger.Level.fatal);
    }
}