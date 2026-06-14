namespace SDClub.Core
{
    public enum GameState
    {
        Bootstrap,
        Login,
        Lobby,
        Playing,
        Pause,
        Result
    }

    [ComponentOf(typeof(Scene))]
    public class GameStateMachineComponent : Entity, IAwake
    {
        public GameState CurrentState { get; set; }

        public GameState PreviousState { get; set; }
    }

    public struct OnGameStateChanged
    {
        public GameState OldState;
        public GameState NewState;
    }
}
