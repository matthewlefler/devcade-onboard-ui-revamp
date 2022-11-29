using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;

namespace onboard.util; 

public static class Cmd {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    private static int exec(string cmd, string args) {
        logger.Trace("executing: " + cmd + " " + args);
        using Process proc = Process.Start(cmd, args);
        proc?.WaitForExit();
        return proc?.ExitCode ?? 1;
    }

    private static int exec(string cmd, string[] args) {
        args = args.Select(arg => arg.Contains(" ") ? $"\"{arg}\"" : arg).ToArray();
        return exec(cmd, string.Join(" ", args));
    }

    #region chmod
    public static int chmod(string path, int mode) {
        return chmod(path, mode, false);
    }
    
    public static int chmod(string path, int mode, bool recursive) {
        return recursive ? exec("/usr/bin/chmod", "-R " + mode + " " + path) : exec("/usr/bin/chmod", mode + " " + path);
    }
    
    public static int chmod(string path, string mode) {
        return chmod(path, mode, false);
    }
    
    public static int chmod(string path, string mode, bool recursive) {
        return recursive ? exec("/usr/bin/chmod", "-R " + mode + " " + path) : exec("/usr/bin/chmod", mode + " " + path);
    }
    #endregion
}