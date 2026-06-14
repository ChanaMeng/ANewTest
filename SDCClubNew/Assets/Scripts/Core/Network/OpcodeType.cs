using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class OpcodeType : Singleton<OpcodeType>, ISingletonAwake
    {
        private readonly Dictionary<ushort, Type> opcodeTypes = new Dictionary<ushort, Type>();
        private readonly Dictionary<Type, ushort> typeOpcodes = new Dictionary<Type, ushort>();

        public void Awake()
        {
        }

        public void Register(ushort opcode, Type type)
        {
            if (this.opcodeTypes.ContainsKey(opcode))
            {
                throw new Exception($"Duplicate opcode: {opcode}, type: {type}");
            }
            if (this.typeOpcodes.ContainsKey(type))
            {
                throw new Exception($"Duplicate type: {type}, opcode: {opcode}");
            }

            this.opcodeTypes[opcode] = type;
            this.typeOpcodes[type] = opcode;
        }

        public ushort GetOpcode(Type type)
        {
            if (!this.typeOpcodes.TryGetValue(type, out ushort opcode))
            {
                throw new Exception($"Type not found in OpcodeType: {type.FullName}");
            }
            return opcode;
        }

        public Type GetType(ushort opcode)
        {
            if (!this.opcodeTypes.TryGetValue(opcode, out Type type))
            {
                Log.Error($"Opcode not found: {opcode}");
                return null;
            }
            return type;
        }
    }
}
