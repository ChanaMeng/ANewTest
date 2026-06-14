using System;

namespace SDClub.Core
{
    public interface ILog
    {
        void Debug(string message);

        void Error(string message);

        void Error(Exception e);

        void Warning(string message);

        void Info(string message);
    }

    public class UnityLogger : ILog
    {
        public void Debug(string message)
        {
            UnityEngine.Debug.Log($"[SDClub] {message}");
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError($"[SDClub] {message}");
        }

        public void Error(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning($"[SDClub] {message}");
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log($"[SDClub] {message}");
        }
    }
}
