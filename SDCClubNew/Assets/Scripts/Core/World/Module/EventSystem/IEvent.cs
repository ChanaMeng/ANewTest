using System;
using Cysharp.Threading.Tasks;

namespace SDClub.Core
{
    public interface IEvent
    {
        Type Type { get; }
    }

    public abstract class AEvent<S, A> : IEvent where S : class, IScene where A : struct
    {
        public Type Type
        {
            get
            {
                return typeof(A);
            }
        }

        protected abstract UniTask Run(S scene, A a);

        public async UniTask Handle(S scene, A a)
        {
            try
            {
                await Run(scene, a);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected virtual void RunSync(S scene, A a) { }

        public void HandleSync(S scene, A a)
        {
            try
            {
                RunSync(scene, a);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
