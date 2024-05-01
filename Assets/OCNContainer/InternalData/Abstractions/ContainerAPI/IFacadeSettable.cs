namespace OCNContainer.InternalData
{
    /// <summary>
    /// Requires for RegistrationData to set Facade to container without direct reference to it
    /// </summary>
    public interface IFacadeSettable
    {
        public void SetFacade(RegistrationData registration);
    }
}