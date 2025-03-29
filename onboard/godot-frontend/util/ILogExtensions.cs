using System;
using log4net;

public static class ILogExtentions {
    public static void Trace(this ILog log, string message, Exception exception) {
        log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            log4net.Core.Level.Trace, message, exception);
    }

    public static void Trace(this ILog log, string message) {
        log.Trace(message, null);
    }

    public static void Verbose(this ILog log, string message, Exception exception) {
        log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            log4net.Core.Level.Verbose, message, exception);
    }

    public static void Verbose(this ILog log, string message) {
        log.Verbose(message, null);
    }

    public static void Log(this ILog log, log4net.Core.Level level, string message, Exception exception) {
        log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            level, message, exception);
    }

    public static void Log(this ILog log, log4net.Core.Level level, string message) {
        log.Log(level, message, null);
    }
}