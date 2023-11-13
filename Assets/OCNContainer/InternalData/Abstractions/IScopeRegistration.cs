using System;
using OCNContainer.InternalData;

namespace OCNContainer
{
    public partial interface IScopeRegistration
    {
        public IRegistrationBuilder Register<T1>() where T1 : class, new();
        
        public void RegisterFromInstance<T1>( T1 instance1) where T1 : class;
        
        public void Bind<TImplementation, TInterface>() where TImplementation : class, TInterface, new();
        
        public void BindFromInstance<TImplementation, TInterface>(TImplementation instance) where TImplementation : class, TInterface, new();
        
        public void RegisterSubContainer<T>(Action<IScopeRegistration> subContainer);
    }
    /// <summary>
    /// Requires for RegistrationData to set Facade to container without direct reference to it
    /// </summary>
    public interface IFacadeSettable
    {
        public void SetFacade(RegistrationData registration);
    }
}