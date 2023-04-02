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
            GlobalContext.Properties["LogFilePath"] = $"{Env.get("DEVCADE_PATH").unwrap_or("/tmp/devcade")}/logs";
            GlobalContext.Properties["LogFileName"] = ".log";
            log4net.Config.XmlConfigurator.Configure();
            LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName).Info("Starting application");
            
            // Set namespace log levels
            LogConfig.init();

            // Application setup
            Client.init();
            
            using var game = new ui.Devcade();
            game.Run();

            // using var game = new Game1();
            // game.Run();
        }
    }
}
