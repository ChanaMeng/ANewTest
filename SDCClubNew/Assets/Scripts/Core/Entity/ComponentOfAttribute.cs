using System;

namespace SDClub.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentOfAttribute : Attribute
    {
        public Type ParentType { get; }

        public ComponentOfAttribute(Type parentType)
        {
            this.ParentType = parentType;
        }
    }
}
