using SDClub.Core;

namespace SDClub.Hotfix
{
    [EntitySystem]
    public class GameStateMachineAwakeSystem : AwakeSystem<GameStateMachineComponent>
    {
        protected override void Awake(GameStateMachineComponent self)
        {
            self.CurrentState = GameState.Bootstrap;
            self.PreviousState = GameState.Bootstrap;
        }
    }
}
