using System;

namespace SDClub.Model
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageAttribute : Attribute
    {
        public ushort Opcode { get; }

        public MessageAttribute(ushort opcode)
        {
            Opcode = opcode;
        }
    }
}
