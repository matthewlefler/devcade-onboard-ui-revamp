using Godot;
using System;
using System.IO;

public partial class LogConfigAutoLoad : Node
{
    /// <summary>
    /// load the config file as early as possible
    /// </summary>
    public LogConfigAutoLoad()
    {
        log4net.Config.XmlConfigurator.Configure(new FileInfo("app.config"));
    }
}
