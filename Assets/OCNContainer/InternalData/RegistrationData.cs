using System;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public class RegistrationData : IRegistrationLazyFacadeState
    {
        public Type CurrentType { get; private set; }
        public object Obj => _obj ??= _objFactoryMethod();
        public bool IsLazy { get; private set; } = false;
        public bool IsAlreadyRegisteredInGameCycle { get; private set; } = false;
        public bool RegisteredFromImplementation { get; private set; }
        public bool IsFacade { get; private set; } = false;

        private object _obj;
        private Func<object> _objFactoryMethod;

        private Container currentContainer;
        private static IFacadeSettable _facadeSettable;
        
        /// <summary>
        /// Called on Object creation lifecycle phase
        /// </summary>
        public void CreateObject()
        {
            if (IsLazy) return;
            
            if (_obj == null)
            {
                _obj = _objFactoryMethod();
            }
            else
            {
                Debug.LogError($"Trying to create multiple objects of type {CurrentType}");
            }
        }
        
        void IRegistrationFacadeState.SetFacade()
        {
            _facadeSettable.SetFacade(this);
            IsFacade = true;
        }

        void IRegistrationLazyState.SetLazy()
        {
            throw new NotImplementedException();
        }

        public static RegistrationData CreateFromImplementationAsSingle<T>(IFacadeSettable facadeSettable) where T : class, new()
        {
            _facadeSettable = facadeSettable;
            
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = true,
                _objFactoryMethod = () => new T()
            };
        }

        public static RegistrationData CreateFromInterfaceAsSingle<T>(IFacadeSettable facadeSettable) where T : class, new()
        {
            _facadeSettable = facadeSettable;
            
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = false,
                _objFactoryMethod = () => new T()
            };
        }

        public static RegistrationData CreateFromImplementationWithInstance<T>(IFacadeSettable facadeSettable, object obj)
        {
            _facadeSettable = facadeSettable;
            
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = true,
                _objFactoryMethod = () => obj
            };
        }

        public static RegistrationData CreateFromInterfaceWithInstance<T>(IFacadeSettable facadeSettable, object obj)
        {
            _facadeSettable = facadeSettable;
            
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = false,
                _objFactoryMethod = () => obj
            };
        }

        
    }
}

namespace OCNContainer.InternalData
{
    public interface IRegistrationLazyState
    {
        public void SetLazy();
    }

    public interface IRegistrationFacadeState
    {
        public void SetFacade();
    }
    //Builder for Lazy or Facade types of registrations
    public interface IRegistrationLazyFacadeState : IRegistrationFacadeState, IRegistrationLazyState
    {
        
    }
}