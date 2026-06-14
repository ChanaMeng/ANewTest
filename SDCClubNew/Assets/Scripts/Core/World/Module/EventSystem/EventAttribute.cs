namespace SDClub.Core
{
    public class EventAttribute : BaseAttribute
    {
        public SceneType SceneType { get; }

        public EventAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}
