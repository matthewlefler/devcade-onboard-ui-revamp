using System.Collections.Generic;
using System.Reflection;
using log4net;
using log4net.Appender;

namespace onboard; 

public static class LogConfig {
    private enum Level {
        INHERIT,
        TRACE,
        VERBOSE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
    }
    
    private class NS {
        public string fullName { get; init; }
        public Level level { get; private set; }
        private NS[] children { get; init; }
        
        public NS(string fullName, Level level, params NS[] children) {
            this.fullName = fullName;
            this.level = level;
            this.children = children;
        }

        public NS(string fullname, Level level) {
            this.fullName = fullname;
            this.level = level;
            this.children = null;
        }

        private void set(Level level) {
            this.level = level;
        }

        public void cascade() {
            if (children == null) return;
            foreach (NS ns in children) {
                if (ns.level == Level.INHERIT) {
                    ns.set(level);
                }
                ns.cascade();
            }
        }
        
        public IEnumerable<NS> flatten() {
            var list = new List<NS> { this };
            if (children == null) return list;
            foreach (NS ns in children) {
                list.AddRange(ns.flatten());
            }
            return list;
        }
    }

    public static void init() {
        root.cascade();
        var list = root.flatten();
        foreach (NS ns in list) {
            ILog logger = LogManager.GetLogger(ns.fullName);
            log4net.Core.Level level = ns.level switch {
                Level.TRACE => log4net.Core.Level.Trace,
                Level.VERBOSE => log4net.Core.Level.Verbose,
                Level.DEBUG => log4net.Core.Level.Debug,
                Level.INFO => log4net.Core.Level.Info,
                Level.WARN => log4net.Core.Level.Warn,
                Level.ERROR => log4net.Core.Level.Error,
                Level.FATAL => log4net.Core.Level.Fatal,
                _ => log4net.Core.Level.All,
            };
            
            // what the fuck is this line? why does it work?
            ((log4net.Repository.Hierarchy.Logger)logger.Logger).Level = level;
        }
    }
    
    // This is the config for the logger for all namespaces. A log level of INHERIT means that the log level of the parent namespace will be used.
    // The log level of the root namespace is the default log level for all namespaces.
    private static readonly NS root = new(
        "onboard",
        Level.DEBUG,
        new NS(
            "onboard.devcade",
            Level.INHERIT,
            new NS(
                "onboard.devcade.Client",
                Level.INHERIT
            ),
            new NS(
                "onboard.devcade.DevcadeAPI",
                Level.INHERIT
            )
        ),
        new NS(
            "onboard.ui",
            Level.INHERIT,
            new NS(
                "onboard.ui.Devcade",
                Level.INHERIT
            ),
            new NS(
                "onboard.ui.Menu",
                Level.INHERIT
            )
        ),
        new NS(
            "onboard.util",
            Level.INHERIT,
            new NS(
                "onboard.util.Cmd",
                Level.INHERIT
            ),
            new NS(
                "onboard.util.Container",
                Level.INHERIT
            ),
            new NS(
                "onboard.util.Network",
                Level.INHERIT
            ),
            new NS(
                "onboard.util.Zip",
                Level.INHERIT
            )
        )
    );
}