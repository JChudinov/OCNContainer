using System;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public class RegistrationData
    {
        public Type CurrentType { get; private set; }
        public object Obj => _obj ??= _creationMethod();
        public bool IsLazy { get; private set; } = false;
        public bool IsAlreadyRegisteredInGameCycle { get; private set; } = false;
        public bool RegisteredFromImplementation { get; private set; }
        public bool IsFacade { get; private set; } = false;

        private object _obj;
        private Func<object> _creationMethod;

        public void CreateObject()
        {
            if (IsLazy) return;
            
            if (_obj == null)
            {
                _obj = _creationMethod();
            }
            else
            {
                Debug.LogError($"Trying to create multiple objects of type {CurrentType}");
            }
        }

        public static RegistrationData CreateFromImplementationAsSingle<T>() where T : class, new()
        {
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = true,
                _creationMethod = () => new T()
            };
        }

        public static RegistrationData CreateFromInterfaceAsSingle<T>() where T : class, new()
        {
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = false,
                _creationMethod = () => new T()
            };
        }

        public static RegistrationData CreateFromImplementationWithInstance<T>(object obj)
        {
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = true,
                _creationMethod = () => obj
            };
        }

        public static RegistrationData CreateFromInterfaceWithInstance<T>(object obj)
        {
            return new RegistrationData()
            {
                CurrentType = typeof(T),
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                RegisteredFromImplementation = false,
                _creationMethod = () => obj
            };
        }
    }
}