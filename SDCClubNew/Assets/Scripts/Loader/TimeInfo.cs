using SDClub.Core;

namespace SDClub.Loader
{
    /// <summary>
    /// 时间信息单例，提供 DeltaTime 和 Time
    /// </summary>
    public class TimeInfo : Singleton<TimeInfo>, ISingletonAwake
    {
        public float DeltaTime { get; private set; }
        public float Time { get; private set; }

        public void Awake()
        {
        }

        public void Update()
        {
            this.DeltaTime = UnityEngine.Time.deltaTime;
            this.Time = UnityEngine.Time.time;
        }
    }
}
