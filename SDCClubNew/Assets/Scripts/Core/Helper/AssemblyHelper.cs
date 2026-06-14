using System;
using System.Collections.Generic;
using System.Reflection;

namespace SDClub.Core
{
    public static class AssemblyHelper
    {
        public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in args)
            {
                Type[] asmTypes;
                try
                {
                    asmTypes = ass.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (Type type in asmTypes)
                {
                    types[type.FullName] = type;
                }
            }

            return types;
        }

        public static Dictionary<string, Type> GetAssemblyTypes(string namespacePrefix)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!ass.FullName.StartsWith(namespacePrefix))
                {
                    continue;
                }

                Type[] asmTypes;
                try
                {
                    asmTypes = ass.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (Type type in asmTypes)
                {
                    types[type.FullName] = type;
                }
            }

            return types;
        }
    }
}
