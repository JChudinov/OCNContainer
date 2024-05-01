namespace OCNContainer
{
    public interface IInitializable
    {
        void OnInitialize(IScope scope);
    }

    public interface ISubscribable
    {
        void OnSubscription();
    }

    public interface IEnableable
    {
        void OnBecameActive(bool b_isActive);
        
    }
    
    public interface IStartable
    {
        void OnStart();
    }

    public interface ITickable
    {
        void OnTick();
    }
}