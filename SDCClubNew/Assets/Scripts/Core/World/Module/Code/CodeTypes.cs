using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class CodeTypes : Singleton<CodeTypes>, ISingletonAwake
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();

        public void Awake()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                if (!asm.FullName.StartsWith("SDClub"))
                {
                    continue;
                }

                Type[] asmTypes;
                try
                {
                    asmTypes = asm.GetTypes();
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (Type type in asmTypes)
                {
                    this.allTypes[type.FullName] = type;

                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    // 记录所有带有 Attribute 的类型，按 Attribute 类型索引
                    object[] attrs = type.GetCustomAttributes(false);
                    foreach (object attr in attrs)
                    {
                        this.types.Add(attr.GetType(), type);
                    }
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            this.allTypes.TryGetValue(typeName, out Type type);
            return type;
        }

        public void CreateCode()
        {
            var hashSet = this.GetTypes(typeof(CodeAttribute));
            foreach (Type type in hashSet)
            {
                object obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj).Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}
