using System;

namespace OCNContainer
{
    public partial interface IScopeRegistration
    {
        public void RegisterSubContainer<T>(Action<IScopeRegistration> subContainer);

        public void Register<T1>() where T1 : class, new();
        
        public void RegisterFromInstance<T1>( T1 instance1) where T1 : class;
        
        public void Bind<TImplementation, TInterface>() where TImplementation : class, TInterface, new();
        
        public void BindFromInstance<TImplementation, TInterface>(TImplementation instance) where TImplementation : class, TInterface, new();
    }
}