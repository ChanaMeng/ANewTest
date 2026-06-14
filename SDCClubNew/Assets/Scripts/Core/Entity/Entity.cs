using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public abstract class Entity : DisposeObject, IPool
    {
        public long InstanceId { get; protected set; }

        public Entity()
        {
        }

        public List<object> CachedUpdateSystems;

        public List<object> CachedLateUpdateSystems;

        protected EntityStatus status = EntityStatus.None;

        public bool IsFromPool
        {
            get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this.status &= ~EntityStatus.IsFromPool;
                }
            }
        }

        protected bool IsRegister
        {
            get => (this.status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
            set
            {
                if (this.IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    this.status |= EntityStatus.IsRegister;
                    this.RegisterSystem();
                }
                else
                {
                    this.status &= ~EntityStatus.IsRegister;
                }
            }
        }

        protected virtual void RegisterSystem()
        {
            this.iScene.Fiber.EntitySystem.RegisterSystem(this);

            // 缓存本实体的系统列表，避免每帧查找
            Type registerType = this.GetType();
            if (this.CachedUpdateSystems == null)
            {
                this.CachedUpdateSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(registerType, typeof(IUpdateSystem)) ?? new List<object>();
            }
            if (this.CachedLateUpdateSystems == null)
            {
                this.CachedLateUpdateSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(registerType, typeof(ILateUpdateSystem)) ?? new List<object>();
            }
        }

        public bool IsComponent
        {
            get => (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsComponent;
                }
                else
                {
                    this.status &= ~EntityStatus.IsComponent;
                }
            }
        }

        protected bool IsCreated
        {
            get => (this.status & EntityStatus.IsCreated) == EntityStatus.IsCreated;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsCreated;
                }
                else
                {
                    this.status &= ~EntityStatus.IsCreated;
                }
            }
        }

        protected bool IsNew
        {
            get => (this.status & EntityStatus.IsNew) == EntityStatus.IsNew;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsNew;
                }
                else
                {
                    this.status &= ~EntityStatus.IsNew;
                }
            }
        }

        public bool IsDisposed => this.InstanceId == 0;

        private Entity parent;

        public virtual Entity Parent
        {
            get => this.parent;
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().FullName}");
                }

                if (value.IScene == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {this.GetType().FullName} {value.GetType().FullName}");
                }

                if (this.parent != null)
                {
                    if (this.parent == value)
                    {
                        Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                        return;
                    }

                    this.parent.RemoveFromChildren(this);
                }

                this.parent = value;
                this.IsComponent = false;
                this.parent.AddToChildren(this);

                if (this is IScene scene)
                {
                    scene.Fiber = this.parent.iScene.Fiber;
                    this.IScene = scene;
                }
                else
                {
                    this.IScene = this.parent.iScene;
                }
            }
        }

        private Entity ComponentParent
        {
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {this.GetType().FullName}");
                }

                if (value.IScene == null)
                {
                    throw new Exception($"cant set parent because parent domain is null: {this.GetType().FullName} {value.GetType().FullName}");
                }

                if (this.parent != null)
                {
                    if (this.parent == value)
                    {
                        Log.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                        return;
                    }

                    this.parent.RemoveFromComponents(this);
                }

                this.parent = value;
                this.IsComponent = true;
                this.parent.AddToComponents(this);

                if (this is IScene scene)
                {
                    scene.Fiber = this.parent.iScene.Fiber;
                    this.IScene = scene;
                }
                else
                {
                    this.IScene = this.parent.iScene;
                }
            }
        }

        public T GetParent<T>() where T : Entity
        {
            return this.Parent as T;
        }

        public long Id { get; protected set; }

        private IScene iScene;

        public virtual IScene IScene
        {
            get
            {
                return this.iScene;
            }
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"domain cant set null: {this.GetType().FullName}");
                }

                if (this.iScene == value)
                {
                    return;
                }

                IScene preScene = this.iScene;
                this.iScene = value;

                if (preScene == null)
                {
                    if (this.InstanceId == 0)
                    {
                        this.InstanceId = IdGenerater.Instance.GenerateInstanceId();
                    }

                    this.IsRegister = true;
                }

                // 递归设置孩子的Domain
                if (this.children != null)
                {
                    foreach (Entity entity in this.children.Values)
                    {
                        entity.IScene = this.iScene;
                    }
                }

                if (this.components != null)
                {
                    foreach (Entity component in this.components.Values)
                    {
                        component.IScene = this.iScene;
                    }
                }

                if (!this.IsCreated)
                {
                    this.IsCreated = true;
                    EntitySystemSingleton.Instance.Deserialize(this);
                }
            }
        }

        /// <summary>
        /// 遍历Parent找到Scene
        /// </summary>
        public IScene Domain
        {
            get
            {
                return this.iScene;
            }
        }

        private Dictionary<long, Entity> children;

        public Dictionary<long, Entity> Children
        {
            get
            {
                return this.children ??= new Dictionary<long, Entity>();
            }
        }

        public virtual void AddToChildren(Entity entity)
        {
            this.Children.Add(entity.Id, entity);
        }

        public virtual void RemoveFromChildren(Entity entity)
        {
            if (this.children == null)
            {
                return;
            }

            this.children.Remove(entity.Id);
        }

        private Dictionary<long, Entity> components;

        public virtual Dictionary<long, Entity> Components
        {
            get
            {
                return this.components ??= new Dictionary<long, Entity>();
            }
        }

        public virtual int ComponentsCount()
        {
            if (this.components == null)
            {
                return 0;
            }
            return this.components.Count;
        }

        public virtual int ChildrenCount()
        {
            if (this.children == null)
            {
                return 0;
            }
            return this.children.Count;
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsRegister = false;
            this.InstanceId = 0;

            // 清理Children
            if (this.children != null)
            {
                foreach (Entity child in this.children.Values)
                {
                    child.Dispose();
                }

                this.children.Clear();
            }

            // 清理Component
            if (this.components != null)
            {
                foreach (var kv in this.components)
                {
                    kv.Value.Dispose();
                }

                this.components.Clear();
            }

            // 触发Destroy事件
            if (this is IDestroy)
            {
                EntitySystemSingleton.Instance.Destroy(this);
            }

            this.iScene = null;

            if (this.parent != null && !this.parent.IsDisposed)
            {
                if (this.IsComponent)
                {
                    this.parent.RemoveComponent(this);
                }
                else
                {
                    this.parent.RemoveFromChildren(this);
                }
            }

            this.parent = null;

            base.Dispose();

            // 把status字段其它的status标记都还原
            bool isFromPool = this.IsFromPool;
            this.status = EntityStatus.None;
            this.IsFromPool = isFromPool;

            EntityPool.Instance.Recycle(this, GetLongHashCode(this.GetType()));
        }

        protected virtual void AddToComponents(Entity component)
        {
            this.Components.Add(this.GetLongHashCode(component.GetType()), component);
        }

        public virtual void RemoveFromComponents(Entity component)
        {
            if (this.components == null)
            {
                return;
            }

            this.components.Remove(this.GetLongHashCode(component.GetType()));
        }

        public virtual K GetChild<K>(long id) where K : Entity
        {
            if (this.children == null)
            {
                return null;
            }

            this.children.TryGetValue(id, out Entity child);
            return child as K;
        }

        public virtual void RemoveChild(long id)
        {
            if (this.children == null)
            {
                return;
            }

            if (!this.children.TryGetValue(id, out Entity child))
            {
                return;
            }

            this.children.Remove(id);
            child.Dispose();
        }

        public virtual void RemoveComponent<K>() where K : Entity
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Type type = typeof(K);

            Entity c;
            if (!this.components.TryGetValue(this.GetLongHashCode<K>(), out c))
            {
                return;
            }

            this.RemoveFromComponents(c);
            c.Dispose();
        }

        public virtual void RemoveComponent(Entity component)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Entity c;
            if (!this.components.TryGetValue(this.GetLongHashCode(component.GetType()), out c))
            {
                return;
            }

            if (c.InstanceId != component.InstanceId)
            {
                return;
            }

            this.RemoveFromComponents(c);
            c.Dispose();
        }

        public virtual void RemoveComponent(Type type)
        {
            if (this.IsDisposed)
            {
                return;
            }

            Entity c;
            if (!this.components.TryGetValue(this.GetLongHashCode(type), out c))
            {
                return;
            }

            RemoveFromComponents(c);
            c.Dispose();
        }

        public virtual K GetComponent<K>() where K : Entity
        {
            if (this.components == null)
            {
                return null;
            }

            Entity component;
            if (!this.components.TryGetValue(this.GetLongHashCode<K>(), out component))
            {
                return default;
            }

            return (K)component;
        }

        public virtual Entity GetComponent(Type type)
        {
            if (this.components == null)
            {
                return null;
            }

            Entity component;
            if (!this.components.TryGetValue(this.GetLongHashCode(type), out component))
            {
                return null;
            }

            return component;
        }

        protected static Entity Create<T>(long id, bool isFromPool) where T : Entity
        {
            Entity component;
            if (isFromPool)
            {
                component = (Entity)EntityPool.Instance.Fetch<T>(id, isFromPool);
            }
            else
            {
                component = Activator.CreateInstance(typeof(T)) as Entity;
            }

            component.IsFromPool = isFromPool;
            component.IsCreated = true;
            component.IsNew = true;
            component.Id = 0;
            return component;
        }

        public Entity AddComponent(Entity component)
        {
            Type type = component.GetType();
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            return component;
        }

        public K AddComponentWithId<K>(long id, bool isFromPool = false, long hashCode = 0) where K : Entity, IAwake, new()
        {
            Type type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            Entity component = Create<K>(hashCode, isFromPool);
            component.Id = id;
            component.ComponentParent = this;

            EntitySystemSingleton.Instance.Awake(component);

            return component as K;
        }

        public K AddComponentWithId<K, P1>(long id, P1 p1, bool isFromPool = false, long hashCode = 0) where K : Entity, IAwake<P1>, new()
        {
            Type type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            Entity component = Create<K>(hashCode, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false, long hashCode = 0) where K : Entity, IAwake<P1, P2>, new()
        {
            Type type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            Entity component = Create<K>(hashCode, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1, p2);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false, long hashCode = 0) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            Type type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            Entity component = Create<K>(hashCode, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1, p2, p3);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2, P3, P4>(long id, P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false, long hashCode = 0) where K : Entity, IAwake<P1, P2, P3, P4>, new()
        {
            Type type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            Entity component = Create<K>(hashCode, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1, p2, p3, p4);

            return component as K;
        }

        public K AddComponent<K>(bool isFromPool = false) where K : Entity, IAwake, new()
        {
            return this.AddComponentWithId<K>(this.Id, isFromPool);
        }

        public K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            return this.AddComponentWithId<K, P1>(this.Id, p1, isFromPool);
        }

        public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            return this.AddComponentWithId<K, P1, P2>(this.Id, p1, p2, isFromPool);
        }

        public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            return this.AddComponentWithId<K, P1, P2, P3>(this.Id, p1, p2, p3, isFromPool);
        }

        public Entity AddChild(Entity entity)
        {
            entity.Parent = this;
            return entity;
        }

        public T AddChild<T>(bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        public T AddChild<T, A>(A a, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake<A>
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a);
            return component;
        }

        public T AddChild<T, A, B>(A a, B b, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake<A, B>
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake<A, B, C>
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = IdGenerater.Instance.GenerateId();
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b, c);
            return component;
        }

        public T AddChildWithId<T>(long id, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = Entity.Create<T>(hashCode, isFromPool) as T;
            component.Id = id;
            component.Parent = this;
            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        public T AddChildWithId<T, A>(long id, A a, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake<A>
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a);
            return component;
        }

        public T AddChildWithId<T, A, B>(long id, A a, B b, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake<A, B>
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b);
            return component;
        }

        public T AddChildWithId<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false, long hashCode = 0) where T : Entity, IAwake<A, B, C>
        {
            Type type = typeof(T);
            hashCode = hashCode == 0 ? this.GetLongHashCode(type) : hashCode;

            T component = (T)Entity.Create<T>(hashCode, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemSingleton.Instance.Awake(component, a, b, c);
            return component;
        }

        public virtual long GetLongHashCode(Type type)
        {
            return type.TypeHandle.Value.ToInt64();
        }

        public virtual long GetLongHashCode<T>()
        {
            return this.GetLongHashCode(typeof(T));
        }
    }
}
