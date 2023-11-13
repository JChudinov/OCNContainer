namespace OCNContainer.InternalData
{
    public interface ILifecycleParticipant
    {
        public void InstanceCreationPhase();
        
        public void ScopeResolvePhase();

        public void EventSubscriptionsPhase();

        public void StartPhase();
    }

    public interface IContainerLifecycleParticipant : ILifecycleParticipant
    {
        public RegistrationData FacadeRegistrationData { get; }
    }
    public interface IInstallBindingPhaseParticipant
    {
        public void InstallBindingsPhase();
    }

    public interface IContainerRecursable
    {
        public void Awake();

        public void Start();
    } 

}