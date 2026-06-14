using System.Collections.Generic;

namespace SDClub.Model
{
    public class MessageSessionDispatcher
    {
        public static MessageSessionDispatcher Instance { get; } = new();
        
        private readonly Dictionary<ushort, IMessageSessionHandler> handlers = new();
        
        public void RegisterHandler(ushort opcode, IMessageSessionHandler handler)
        {
            handlers[opcode] = handler;
        }
        
        public void Dispatch(Session session, ushort opcode, object message)
        {
            if (handlers.TryGetValue(opcode, out var handler))
            {
                handler.Handle(session, message);
            }
        }
    }
}
