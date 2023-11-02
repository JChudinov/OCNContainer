namespace OCNContainer
{
    public interface IInitializable
    {
        void Initialize(IScope scope);
    }

    public interface ISubscribable
    {
        void EventSubscriptions();
    }

    public interface IEnableable
    {
        void OnEnable();

        void OnDisable();
    }
    
    public interface IStartable
    {
        void Start();
    }

    public interface ITickable
    {
        void Tick();
    }
}