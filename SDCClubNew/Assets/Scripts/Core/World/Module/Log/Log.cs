using System;

namespace SDClub.Core
{
    public static class Log
    {
        private static ILog logger = new UnityLogger();

        public static ILog ILog
        {
            get => logger;
            set => logger = value ?? new UnityLogger();
        }

        public static void Debug(string message)
        {
            logger.Debug(message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

        public static void Error(Exception e)
        {
            logger.Error(e);
        }

        public static void Warning(string message)
        {
            logger.Warning(message);
        }

        public static void Info(string message)
        {
            logger.Info(message);
        }
    }
}
