namespace SDClub.Core
{
    public class Scene : Entity, IScene
    {
        public Fiber Fiber { get; set; }

        public string Name { get; private set; }

        public SceneType SceneType
        {
            get;
            set;
        }

        public Scene()
        {
        }

        public Scene(Fiber fiber, long id, long instanceId, SceneType sceneType, string name)
        {
            this.Id = id;
            this.Name = name;
            this.InstanceId = instanceId;
            this.SceneType = sceneType;
            this.IsCreated = true;
            this.IsNew = true;
            this.Fiber = fiber;
            this.IScene = this;
            this.IsRegister = true;
            Log.Debug($"scene create: {this.SceneType} {this.Id} {this.InstanceId}");
        }

        public override void Dispose()
        {
            base.Dispose();

            Log.Debug($"scene dispose: {this.SceneType} {this.Id} {this.InstanceId}");
        }
    }
}
