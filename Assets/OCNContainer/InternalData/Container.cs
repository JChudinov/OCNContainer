using System;

namespace OCNContainer.InternalData
{
    public partial class Container
    {
        void IScopeRegistration.RegisterSubContainer<T>(Action<IScopeRegistration> subContainer) => 
            RegisterSubContainer_Internal<T>(subContainer);

        IRegistrationBuilder IScopeRegistration.Register<T1>() =>
            Register_Internal<T1>();

        void IScopeRegistration.RegisterFromInstance<T>(T instance) =>
            RegisterFromInstance_Internal<T>(instance);
        
        void IScopeRegistration.Bind<TImplementation, TInterface>() =>
            Bind_Internal<TImplementation, TInterface>(true);

        void IScopeRegistration.BindFromInstance<TImplementation, TInterface>(TImplementation instance) =>
            BindFromInstance_Internal<TImplementation, TInterface>(instance, true);

        T IScope.Resolve<T>() where T : class =>
            Resolve_Internal<T>();
    }
}