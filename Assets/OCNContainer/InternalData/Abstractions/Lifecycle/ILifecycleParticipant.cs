namespace OCNContainer.InternalData
{
    public interface ILifecycleParticipant
    {
        public void InstanceCreationPhase();
        
        public void ScopeResolvePhase();

        public void EventSubscriptionsPhase();

        public void StartPhase();

        public void UpdatePhase();
    }
}