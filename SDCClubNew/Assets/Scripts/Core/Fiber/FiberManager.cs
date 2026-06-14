using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class FiberManager : Singleton<FiberManager>, ISingletonAwake
    {
        private readonly Dictionary<int, Fiber> fibers = new();

        public void Awake()
        {
        }

        public void Update()
        {
            foreach (var kv in this.fibers)
            {
                kv.Value.Update();
            }
        }

        public void LateUpdate()
        {
            foreach (var kv in this.fibers)
            {
                kv.Value.LateUpdate();
            }
        }

        public Fiber Create(int fiberId, SceneType sceneType, string name = "")
        {
            Fiber fiber = new(fiberId, sceneType, name);

            if (!this.fibers.TryAdd(fiberId, fiber))
            {
                throw new Exception($"same fiber already existed: {fiberId}");
            }

            return fiber;
        }

        public void Remove(int id)
        {
            if (this.fibers.Remove(id, out Fiber fiber))
            {
                fiber.Dispose();
            }
        }

        public Fiber Get(int id)
        {
            this.fibers.TryGetValue(id, out Fiber fiber);
            return fiber;
        }

        public int Count()
        {
            return this.fibers.Count;
        }

        public override void Dispose()
        {
            foreach (var kv in this.fibers)
            {
                kv.Value.Dispose();
            }

            this.fibers.Clear();
            base.Dispose();
        }
    }
}
