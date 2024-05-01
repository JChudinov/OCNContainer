using System;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public partial class Container
    {
        void IScopeRegistration.RegisterSubContainer<T>(Action<IScopeRegistration> subContainer) => 
            RegisterSubContainer_Internal<T>(subContainer);

        IRegistrationBuilder IScopeRegistration.Register<T1>() =>
            Register_Internal<T1>();

        IRegistrationFromInstanceBuilder IScopeRegistration.RegisterFromInstance<T>(T instance) =>
            RegisterFromInstance_Internal<T>(instance);
        
        IRegistrationFromInstanceBuilder IScopeRegistration.RegisterFromHierarchyResolve<T>() =>
            RegisterFromHierarchyResolve_Internal<T>();
        
        void IScopeRegistration.Bind<TImplementation, TInterface>() =>
            Bind_Internal<TImplementation, TInterface>(true);

        void IScopeRegistration.BindFromInstance<TImplementation, TInterface>(TImplementation instance) =>
            BindFromInstance_Internal<TImplementation, TInterface>(instance, true);

        T IScope.Resolve<T>() where T : class =>
            Resolve_Internal<T>();

        T IScope.FindInHierarchy<T>() => 
            FindInHierarchy_Internal<T>();

        T IScope.AddToLifecycle<T>() => 
            AddToLifecycle_Internal<T>();
    }
}