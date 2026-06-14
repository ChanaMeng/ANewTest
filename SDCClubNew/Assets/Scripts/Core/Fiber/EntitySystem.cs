using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class EntitySystem
    {
        private readonly Queue<Entity> updateQueue = new();
        private readonly Queue<Entity> lateUpdateQueue = new();

        public EntitySystem()
        {
        }

        public virtual void RegisterSystem(Entity component)
        {
            if (component == null)
            {
                return;
            }

            // 根据Entity是否实现了对应接口来决定注册到哪个队列
            if (component is IUpdate)
            {
                this.updateQueue.Enqueue(component);
            }

            if (component is ILateUpdate)
            {
                this.lateUpdateQueue.Enqueue(component);
            }
        }

        public void Update()
        {
            int count = this.updateQueue.Count;
            while (count-- > 0)
            {
                Entity component = this.updateQueue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                if (component is not IUpdate)
                {
                    continue;
                }

                try
                {
                    // 缓存本实体的系统列表
                    if (component.CachedUpdateSystems == null)
                    {
                        this.updateQueue.Enqueue(component);
                        continue;
                    }

                    this.updateQueue.Enqueue(component);

                    foreach (IUpdateSystem iUpdateSystem in component.CachedUpdateSystems)
                    {
                        try
                        {
                            iUpdateSystem.Run(component);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"entity system update fail: {component.GetType().FullName}", e);
                }
            }
        }

        public void LateUpdate()
        {
            int count = this.lateUpdateQueue.Count;
            while (count-- > 0)
            {
                Entity component = this.lateUpdateQueue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }

                if (component is not ILateUpdate)
                {
                    continue;
                }

                List<object> iLateUpdateSystems = component.CachedLateUpdateSystems;
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                this.lateUpdateQueue.Enqueue(component);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }
    }
}
