using System;
using SDClub.Core;

namespace SDClub.Model
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageSessionHandlerAttribute : Attribute
    {
        public SceneType SceneType { get; }
        public MessageSessionHandlerAttribute(SceneType sceneType)
        {
            SceneType = sceneType;
        }
    }
    
    public interface IMessageSessionHandler
    {
        Type GetMessageType();
        void Handle(Session session, object message);
    }
    
    public abstract class MessageSessionHandler<T> : IMessageSessionHandler 
        where T : class, IMessage
    {
        public Type GetMessageType() => typeof(T);
        protected abstract void Run(Session session, T message);
        
        public void Handle(Session session, object message)
        {
            Run(session, message as T);
        }
    }
    
    public abstract class MessageSessionHandler<Req, Res> : IMessageSessionHandler
        where Req : class, IRequest
        where Res : class, IResponse
    {
        public Type GetMessageType() => typeof(Req);
        protected abstract Res Run(Session session, Req request);
        
        public void Handle(Session session, object message)
        {
            var response = Run(session, message as Req);
            session.Send(response);
        }
    }
}
