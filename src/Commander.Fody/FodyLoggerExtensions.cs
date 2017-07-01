using System;
using JetBrains.Annotations;

namespace Commander.Fody
{
    public static class FodyLoggerExtensions
    {        
        public static void Error(this IFodyLogger logger, Exception ex)
        {
            logger.Error("Error: ", ex);
        }

        public static void Error(this IFodyLogger logger, string message, Exception ex)
        {
            logger.Error("{0} {1}", message, ex);
        }

        [StringFormatMethod("format")]
        public static void Error(this IFodyLogger logger, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            logger.LogError(msg);
        }

        [StringFormatMethod("format")]
        public static void Warning(this IFodyLogger logger, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            logger.LogInfo(msg);
        }

        [StringFormatMethod("format")]
        public static void Info(this IFodyLogger logger, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            logger.LogInfo(msg);
        }
    }
}