using System;
using SDClub.Core;

namespace SDClub.Loader
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILog
    {
        void Debug(string message);
        void Warning(string message);
        void Error(string message);
        void Error(Exception e);
    }

    /// <summary>
    /// Unity 日志单例，封装 UnityEngine.Debug
    /// </summary>
    public class UnityLogger : Singleton<UnityLogger>, ISingletonAwake, ILog
    {
        public void Awake()
        {
        }

        public void Debug(string message)
        {
            UnityEngine.Debug.Log($"[SDClub] {message}");
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning($"[SDClub] {message}");
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError($"[SDClub] {message}");
        }

        public void Error(Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
}
