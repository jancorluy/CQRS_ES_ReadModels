using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Internal;
using NLog.Layouts;
using NLog.Targets;

namespace Disruptor.ReadModel.Host
{
    public static class LoggingConfig
    {
        /// <summary>
        ///     Initialize the NLog configuration with the configured targets and log level
        /// </summary>
        public static LoggingConfiguration GetDefault()
        {
            var logLevel = GetLogLevel();

            var config = new LoggingConfiguration();
            
            var coloredConsoleTarget = new ColoredConsoleTarget
            {
                Layout =
                    new SimpleLayout(
                        @"${date:format=HH\:mm\:ss} - ${level:uppercase=true} - ${message} ${exception:innerFormat=ToString,StackTrace:maxInnerExceptionLevel=3:format=ToString,StackTrace}")
            };
            config.AddRule(logLevel, LogLevel.Fatal, coloredConsoleTarget);
#if DEBUG
            var debugTarget = new DebugTarget();
            config.AddRule(logLevel, LogLevel.Fatal, debugTarget);
#endif
            return config;
        }

        public static LogLevel GetLogLevel()
        {
            return LogLevel.Info;
        }

   
    }
}
