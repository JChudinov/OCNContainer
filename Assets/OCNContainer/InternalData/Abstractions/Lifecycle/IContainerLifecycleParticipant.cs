namespace OCNContainer.InternalData
{
    public interface IContainerLifecycleParticipant : ILifecycleParticipant
    {
        public RegistrationData FacadeRegistrationData { get; }
    }
}