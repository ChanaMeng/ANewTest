using System;
using System.Collections.Generic;
using System.Reflection;

namespace SDClub.Core
{
    public class TypeSystems
    {
        public class OneTypeSystems
        {
            public OneTypeSystems(int count)
            {
                this.QueueFlag = new bool[count];
            }

            // SystemType -> List<system instances>
            public readonly Dictionary<Type, List<object>> Map = new();

            // 标记该实体类型需要在哪些队列中更新
            public readonly bool[] QueueFlag;
        }

        private readonly int count;

        public TypeSystems(int count)
        {
            this.count = count;
        }

        private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

        public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
        {
            if (this.typeSystemsMap.TryGetValue(type, out var systems))
            {
                return systems;
            }

            systems = new OneTypeSystems(this.count);
            this.typeSystemsMap.Add(type, systems);
            return systems;
        }

        public OneTypeSystems GetOneTypeSystems(Type type)
        {
            this.typeSystemsMap.TryGetValue(type, out var systems);
            return systems;
        }

        public List<object> GetSystems(Type type, Type systemType)
        {
            if (!this.typeSystemsMap.TryGetValue(type, out var oneTypeSystems))
            {
                return null;
            }

            if (!oneTypeSystems.Map.TryGetValue(systemType, out List<object> systems))
            {
                return null;
            }

            return systems;
        }
    }

    /// <summary>
    /// Entity系统单例，负责扫描[EntitySystem]特性，维护TypeSystems映射表，
    /// 提供Awake/Update/LateUpdate/Destroy等生命周期方法。
    /// </summary>
    public class EntitySystemSingleton : Singleton<EntitySystemSingleton>, ISingletonAwake
    {
        public TypeSystems TypeSystems { get; private set; }

        public void Awake()
        {
            this.TypeSystems = new TypeSystems(InstanceQueueIndex.Max);

            // 扫描所有程序集中带有 [EntitySystem] 特性的类
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (Type type in types)
                {
                    if (type.GetCustomAttribute<EntitySystemAttribute>() == null)
                    {
                        continue;
                    }

                    if (!typeof(ISystemType).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    object obj = Activator.CreateInstance(type);

                    if (obj is ISystemType iSystemType)
                    {
                        TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());

                        if (!oneTypeSystems.Map.TryGetValue(iSystemType.SystemType(), out List<object> list))
                        {
                            list = new List<object>();
                            oneTypeSystems.Map[iSystemType.SystemType()] = list;
                        }

                        list.Add(obj);

                        int index = iSystemType.GetInstanceQueueIndex();
                        if (index > InstanceQueueIndex.None && index < InstanceQueueIndex.Max)
                        {
                            oneTypeSystems.QueueFlag[index] = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 序列化前的回调（简化实现，暂不处理）
        /// </summary>
        public void Serialize(Entity component)
        {
            // 简化实现：无序列化操作
        }

        /// <summary>
        /// 反序列化后的回调（简化实现，暂不处理）
        /// </summary>
        public void Deserialize(Entity component)
        {
            // 简化实现：无序列化操作
        }

        public void Awake(Entity component)
        {
            List<object> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1>(Entity component, P1 p1)
        {
            if (component is not IAwake<P1>)
            {
                return;
            }

            List<object> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
        {
            if (component is not IAwake<P1, P2>)
            {
                return;
            }

            List<object> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
        {
            if (component is not IAwake<P1, P2, P3>)
            {
                return;
            }

            List<object> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2, P3>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (component is not IAwake<P1, P2, P3, P4>)
            {
                return;
            }

            List<object> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2, P3, P4>));
            if (iAwakeSystems == null)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Destroy(Entity component)
        {
            if (component is not IDestroy)
            {
                return;
            }

            List<object> iDestroySystems = this.TypeSystems.GetSystems(component.GetType(), typeof(IDestroySystem));
            if (iDestroySystems == null)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
