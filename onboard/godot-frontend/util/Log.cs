using Godot;

namespace onboard.util; 

public class Logger
{
    public string className {get; private set;}= "null";

    public Logger(string className)
    {
        this.className = className;
    }

    /// <summary>
    /// Logs (prints) an arbitrary message at a given log level
    /// </summary>
    /// <param name="msg"> the message to send </param>
    /// <param name="logLevel"> The level to log </param>
    public void logMessage(string msg, Log.Level logLevel)
    {
        string time = Time.GetTimeStringFromSystem();

        string message = $"[{time} {Log.stringWrapLogLevelColor(logLevel.ToString().ToUpperInvariant(), logLevel)} {className}] {msg}";

        Log.logMessage(message, logLevel);
    }

    // trace, verbose, debug, info, warn, error, fatal methods
    
    public void Trace(string msg)
    {
        logMessage(msg, Log.Level.trace);
    }

    public void Verbose(string msg)
    {
        logMessage(msg, Log.Level.verbose);
    }

    public void Debug(string msg)
    {
        logMessage(msg, Log.Level.debug);
    }

    public void Info(string msg)
    {
        logMessage(msg, Log.Level.info);
    }

    public void Warn(string msg)
    {
        logMessage(msg, Log.Level.warn);
    }

    public void Error(string msg)
    {
        logMessage(msg, Log.Level.error);
    }

    public void Fatal(string msg)
    {
        logMessage(msg, Log.Level.fatal);
    }
}