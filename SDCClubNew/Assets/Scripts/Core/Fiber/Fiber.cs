namespace SDClub.Core
{
    public class Fiber
    {
        public int Id { get; private set; }
        public SceneType SceneType { get; private set; }
        public string Name { get; private set; }
        public Scene Root { get; }
        public EntitySystem EntitySystem { get; }

        public Fiber(int id, SceneType sceneType, string name)
        {
            this.Id = id;
            this.SceneType = sceneType;
            this.Name = name;
            this.EntitySystem = new EntitySystem();
            this.Root = new Scene(this, id, IdGenerater.Instance.GenerateInstanceId(), sceneType, name);
        }

        public void Update()
        {
            this.EntitySystem.Update();
        }

        public void LateUpdate()
        {
            this.EntitySystem.LateUpdate();
        }

        public void Dispose()
        {
            this.Root?.Dispose();
        }
    }
}
