namespace SDClub.Core
{
    public interface IScene
    {
        Fiber Fiber { get; set; }
        SceneType SceneType { get; set; }
    }
}
