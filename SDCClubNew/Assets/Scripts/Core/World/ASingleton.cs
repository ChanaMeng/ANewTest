namespace SDClub.Core
{
    public interface ISingletonReverseDispose
    {
    }

    public abstract class ASingleton : DisposeObject
    {
        internal abstract void Register();
    }
}
