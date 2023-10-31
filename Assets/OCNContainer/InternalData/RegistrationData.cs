using System;

namespace OCNContainer.InternalData
{
    public class RegistrationData
    {
        public Type CurrentType;
        public object Obj;
        public bool IsLazy = false;
        public bool IsAlreadyRegisteredInGameCycle = false;
        public bool RegisteredFromImplementation;
        public bool IsFacade = false;

        private RegistrationData()
        {
            
        }
        
        private RegistrationData(Type type, bool registeredFromImplementation, bool isLazy = false, bool isFacade = false)
        {
            CurrentType = type;
            RegisteredFromImplementation = registeredFromImplementation;
            IsLazy = isLazy;
            IsFacade = isFacade;
        }

        private RegistrationData(Type type, object instance, bool registeredFromImplementation, bool isFacade = false)
        {
            CurrentType = type;
            Obj = instance;
            RegisteredFromImplementation = registeredFromImplementation;
            IsLazy = false;
            IsFacade = isFacade;
        }

        public static RegistrationData CreateFromImplementationAsSingle(Type type)
        {
            return new RegistrationData()
            {
                CurrentType = type,
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                Obj = null,
                RegisteredFromImplementation = true
            };
        }

        public static RegistrationData CreateFromInterfaceAsSingle(Type type)
        {
            return new RegistrationData()
            {
                CurrentType = type,
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                Obj = null,
                RegisteredFromImplementation = false
            };
        }

        public static RegistrationData CreateFromImplementationWithInstance(Type type, object obj)
        {
            return new RegistrationData()
            {
                CurrentType = type,
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                Obj = obj,
                RegisteredFromImplementation = true
            };
        }

        public static RegistrationData CreateFromInterfaceWithInstance(Type type, object obj)
        {
            return new RegistrationData()
            {
                CurrentType = type,
                IsAlreadyRegisteredInGameCycle = false,
                IsFacade = false,
                IsLazy = false,
                Obj = obj,
                RegisteredFromImplementation = false
            };
        }
    }
}