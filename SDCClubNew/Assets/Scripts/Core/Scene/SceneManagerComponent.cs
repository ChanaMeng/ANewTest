namespace SDClub.Core
{
    public enum SceneLoadState
    {
        Idle,
        Loading,
        Loaded,
        Failed
    }

    [ComponentOf(typeof(Scene))]
    public class SceneManagerComponent : Entity, IAwake<string>
    {
        public string CurrentScene { get; set; }

        public SceneLoadState LoadState { get; set; }

        public float Progress { get; set; }
    }

    public struct OnSceneLoading
    {
        public string SceneName;
        public float Progress;
    }

    public struct OnSceneLoaded
    {
        public string SceneName;
    }
}
