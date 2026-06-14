using Cysharp.Threading.Tasks;
using SDClub.Core;
using UnityEngine.SceneManagement;

namespace SDClub.Hotfix
{
    [EntitySystem]
    public class SceneManagerAwakeSystem : AwakeSystem<SceneManagerComponent, string>
    {
        protected override void Awake(SceneManagerComponent self, string sceneName)
        {
            self.LoadState = SceneLoadState.Idle;
            self.Progress = 0f;
            self.CurrentScene = sceneName;
        }
    }

    public static class SceneManagerHelper
    {
        public static async UniTask LoadSceneAsync(SceneManagerComponent self)
        {
            var sceneName = self.CurrentScene;
            if (string.IsNullOrEmpty(sceneName))
            {
                Log.Error("SceneManagerComponent: no scene name configured");
                return;
            }

            self.LoadState = SceneLoadState.Loading;
            self.Progress = 0f;

            EventSystem.Instance.PublishSync(self.IScene, new OnSceneLoading
            {
                SceneName = sceneName,
                Progress = 0f
            });

            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                self.LoadState = SceneLoadState.Failed;
                Log.Error($"SceneManagerComponent: scene '{sceneName}' not found in build settings.");
                return;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                self.Progress = operation.progress;
                EventSystem.Instance.PublishSync(self.IScene, new OnSceneLoading
                {
                    SceneName = sceneName,
                    Progress = operation.progress
                });
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            operation.allowSceneActivation = true;
            await operation;

            self.LoadState = SceneLoadState.Loaded;
            self.Progress = 1f;

            EventSystem.Instance.PublishSync(self.IScene, new OnSceneLoaded
            {
                SceneName = sceneName
            });
        }
    }
}
