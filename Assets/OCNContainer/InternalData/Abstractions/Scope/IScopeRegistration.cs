using System;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public partial interface IScopeRegistration
    {
        public IRegistrationBuilder Register<T1>() where T1 : class, new();
        
        public IRegistrationFromInstanceBuilder RegisterFromInstance<T1>( T1 instance1) where T1 : class;

        public IRegistrationFromInstanceBuilder RegisterFromHierarchyResolve<T1>() where T1 : MonoBehaviour;
        
        public void Bind<TImplementation, TInterface>() where TImplementation : class, TInterface, new();
        
        public void BindFromInstance<TImplementation, TInterface>(TImplementation instance) where TImplementation : class, TInterface, new();
        
            
        public void RegisterSubContainer<T>(Action<IScopeRegistration> subContainer);
        
    }
    
}