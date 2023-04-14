using System;
using System.Reflection;
using log4net;
using onboard.devcade;
using onboard.util;

namespace onboard
{
    public static class Program
    {
        [STAThread]
        private static void Main() {
            Env.load("../.env");

            // Logging setup
            GlobalContext.Properties["LogFilePath"] = $"{Env.get("DEVCADE_PATH").unwrap_or("/tmp/devcade")}/logs/frontend";
            GlobalContext.Properties["LogFileName"] = ".log";
            log4net.Config.XmlConfigurator.Configure();
            LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName).Info("Starting application");
            
            LogConfig.Level level = Env.get("FRONTEND_LOG").unwrap_or_else(() => Env.get("RUST_LOG").unwrap_or("INFO")).ToUpper() switch {
                "TRACE" => LogConfig.Level.TRACE,
                "VERBOSE" => LogConfig.Level.VERBOSE,
                "DEBUG" => LogConfig.Level.DEBUG,
                "INFO" => LogConfig.Level.INFO,
                "WARN" => LogConfig.Level.WARN,
                "ERROR" => LogConfig.Level.ERROR,
                "FATAL" => LogConfig.Level.FATAL,
                _ => LogConfig.Level.INFO,
            };
            
            // Set namespace log levels
            LogConfig.init(level);

            // Application setup
            Client.init();
            
            using var game = new ui.Devcade();
            game.Run();
        }
    }
}
