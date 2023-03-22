using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using log4net;

namespace onboard.util; 

public static class Cmd {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    public static int exec(string cmd, string args) {
        logger.Trace("executing: " + cmd + " " + args);
        using Process proc = Process.Start(cmd, args);
        proc?.WaitForExit();
        return proc?.ExitCode ?? 1;
    }

    public static int exec(string cmd, string args, ILog logger, log4net.Core.Level stdOutLevel = null, log4net.Core.Level stdErrLevel = null) {
        Cmd.logger.Trace("executing: \"" + cmd + " " + args + "\" with logged output");
        using var proc = new Process();
        proc.StartInfo.FileName = cmd;
        proc.StartInfo.Arguments = args;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        if (stdOutLevel == null) {
            stdOutLevel = log4net.Core.Level.Info;
        }
        if (stdErrLevel == null) {
            stdErrLevel = log4net.Core.Level.Error;
        }
        proc.OutputDataReceived += (_, e) => {
            if (e.Data != null) {
                logger.Log(stdOutLevel, e.Data);
            }
        };
        proc.ErrorDataReceived += (_, e) => {
            if (e.Data != null) {
                logger.Log(stdErrLevel, e.Data);
            }
        };
        proc.Start();
        proc.WaitForExit();
        string stdOut = proc.StandardOutput.ReadToEnd();
        string stdErr = proc.StandardError.ReadToEnd();
        if (stdOut.Length > 0) {
            logger.Log(stdOutLevel, stdOut);
        }
        if (stdErr.Length > 0) {
            logger.Log(stdErrLevel, stdErr);
        }
        return proc.ExitCode;
    }

    public static int exec(string cmd, string[] args) {
        args = args.Select(arg => arg.Contains(" ") ? $"\"{arg}\"" : arg).ToArray();
        return exec(cmd, string.Join(" ", args));
    }
    
    public static int exec(string cmd, string[] args, ILog logger, log4net.Core.Level stdOutLevel = null, log4net.Core.Level stdErrLevel = null) {
        args = args.Select(arg => arg.Contains(" ") ? $"\"{arg}\"" : arg).ToArray();
        return exec(cmd, string.Join(" ", args), logger, stdOutLevel, stdErrLevel);
    }
    
    public static Task<int> execAsync(string cmd, string args) {
        return Task.Run(() => exec(cmd, args));
    }
    
    public static Task<int> execAsync(string cmd, string args, ILog logger, log4net.Core.Level stdOutLevel = null, log4net.Core.Level stdErrLevel = null) {
        return Task.Run(() => exec(cmd, args, logger, stdOutLevel, stdErrLevel));
    }
    
    public static Task<int> execAsync(string cmd, string[] args) {
        return Task.Run(() => exec(cmd, args));
    }
    
    public static Task<int> execAsync(string cmd, string[] args, ILog logger, log4net.Core.Level stdOutLevel = null, log4net.Core.Level stdErrLevel = null) {
        return Task.Run(() => exec(cmd, args, logger, stdOutLevel, stdErrLevel));
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