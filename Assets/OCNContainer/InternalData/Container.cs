using System;
using System.Collections.Generic;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer.InternalData
{

    public partial class Container : ILifecycleParticipant, IScope, IScopeRegistration
    {
        private Dictionary<Type, object> _registrationDataDictionary = new();
        private Dictionary<Type, Type> _interfaceToTypeDictionary = new();

        private List<ITickable> _updeablePool = new();
        private List<IStartable> _startablePool = new();
        private List<IInitializable> _awakeablePool = new();
        private List<ISubscribable> _subscribablePool = new();

        //for debugging
        private string DebugInformationString => $"in Installer: \"{_installerType}\" on GameObject: \"{_bindedGameObject.name}\"";
        private GameObject _bindedGameObject;
        private Type _installerType;

        public Container(GameObject bindedGameObject, Type installerType)
        {
            _bindedGameObject = bindedGameObject;
            _installerType = installerType;

            foreach (var awakeable in _awakeablePool)
            {
                awakeable.Initialize(this);
            }
        }

        
        
        void IScopeRegistration.RegisterFromInstance<T>(T instance) where T : class
        {
            RegisterFromInstance_Internal<T>(instance, false);
        }
        
        private void RegisterFromInstance_Internal<T>(T instance, bool isFacade = false) where T : class
        {
            if (instance == null)
            {
                Debug.LogError($"Cant register null instance of type: \"{typeof(T).Name}\"" + DebugInformationString);
                return;
            }

            //var registrationData = new RegistrationData(typeof(T), instance, true, true);
            var registrationData = RegistrationData.CreateFromImplementationWithInstance(typeof(T), instance);
            
            if (_registrationDataDictionary.TryAdd(typeof(T), registrationData))
            {
                AddToFullGameCycle(registrationData);
            }
            else
            {
                Debug.LogError(
                    $"Type: {typeof(T)} already registered " + DebugInformationString);
            }
        }
        
        private void Bind_Internal<TImplementation, TInterface>(bool isLazy = false)
            where TImplementation : class, TInterface, new()
        {
            if (typeof(TImplementation).IsSubclassOf(typeof(MonoBehaviour)))
            {
                Debug.LogError(
                    $"Unable to Register subclass of MonoBehaviour: \"{typeof(TImplementation)}\" without providing and instance" +
                    DebugInformationString);
                return;
            }


            if (_interfaceToTypeDictionary.TryAdd(typeof(TInterface), typeof(TImplementation)))
            {
                var registrationData = RegistrationData.CreateFromInterfaceAsSingle(typeof(TImplementation));
                
                if (_registrationDataDictionary.TryAdd(typeof(TImplementation), registrationData))
                {
                    if (isLazy == false)
                    {
                        registrationData.Obj = new TImplementation();
                        AddToFullGameCycle(registrationData);
                    }
                    else
                    {
                        throw new Exception("Not yet implemented");
                    }
                }
            }
            else
            {
                Debug.LogError(
                    $"Type {typeof(TInterface)} already registered as interface " + DebugInformationString);
            }
        }
        
        private void BindFromInstance_Internal<TImplementation, TInterface>(TImplementation instance)
            where TImplementation : class, TInterface, new()
        {
            if (instance == null)
            {
                Debug.LogError($"Cant register null instance of type: \"{typeof(TImplementation)}\"" + DebugInformationString);
                return;
            }


            if (_interfaceToTypeDictionary.TryAdd(typeof(TInterface), typeof(TImplementation)))
            {
                //var registrationData = new RegistrationData(typeof(TImplementation), false);
                var registrationData = RegistrationData.CreateFromInterfaceWithInstance(typeof(TImplementation), instance);
                if (_registrationDataDictionary.TryAdd(typeof(TImplementation), registrationData))
                {
                    AddToFullGameCycle(registrationData);
                }
            }
            else
            {
                Debug.LogError(
                    $"Type {typeof(TInterface)} already registered as interface " + DebugInformationString);
            }
        }


        private void Register_Internal<T>(bool isLazy = false, bool isFacade = false) where T : class, new()
        {
            if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                Debug.LogError(
                    $"Unable to Register subclass of MonoBehaviour: \"{typeof(T)}\" without providing and instance " + DebugInformationString);
                return;
            }

            var tmpRegData = RegistrationData.CreateFromImplementationAsSingle(typeof(T));
            if (_registrationDataDictionary.TryAdd(typeof(T), tmpRegData))
            {
                if (isLazy == false)
                {
                    tmpRegData.IsLazy = false;
                    tmpRegData.Obj = new T();
                    AddToFullGameCycle(tmpRegData);
                }
                //TODO: add facade check
            }
            else
            {
                Debug.LogError(
                    $"Type: {typeof(T).Name} already registered " + DebugInformationString);
            }
        }
        
        public T Resolve<T>() where T : class
        {
            Type typeToResolve = typeof(T);
            bool resolvedFromInterface = false;

            if (_interfaceToTypeDictionary.TryGetValue(typeof(T), out Type relatedImplementation))
            {
                typeToResolve = relatedImplementation;
                resolvedFromInterface = true;
            }

            if (TryFindRegistrationData(typeToResolve, out var registrationData))
            {
                if (registrationData.Obj != null)
                {
                    if (resolvedFromInterface == false && registrationData.RegisteredFromImplementation == false)
                    {
                        Debug.LogError(
                            $"Cant resolve concrete type \" {typeToResolve}\" as it was registered as interface " + DebugInformationString);
                        return null;
                    }

                    if (resolvedFromInterface == true && registrationData.RegisteredFromImplementation == true)
                    {
                        Debug.LogError(
                            $"Interface: \"{typeof(T)}\" is not associated in container with type: \"{typeToResolve}\" " +
                            DebugInformationString);
                    }

                    return registrationData.Obj as T;
                }
                else
                {
                    Debug.LogError($"Instance of type \"{typeToResolve}\" was not registered correctly " + DebugInformationString);
                    return null;
                }
            }
            else
            {
                Debug.LogError(
                    $"Unable to retrieve proper registration data related for type: \"{typeToResolve.Name}\" " + DebugInformationString);
                return null;
            }
        }
//TODO:
//perhaps, both return statements should be logged as different errors
        private bool TryFindRegistrationData(Type type, out RegistrationData validRegistrationData)
        {
            if (_registrationDataDictionary.TryGetValue(type, out var registrationData_unspecified))
            {
                if (registrationData_unspecified is not RegistrationData registrationData)
                {
                    validRegistrationData = null;
                    return false;
                }

                validRegistrationData = registrationData;
                return true;
            }
            validRegistrationData = null;
            return false;
        }

        private void AddToFullGameCycle(RegistrationData registrationData)
        {
            if (registrationData.IsLazy && registrationData.Obj is IInitializable or IStartable)
            {
                Debug.LogError("Trying to add Lazy binding of class ");
            }

            var obj = registrationData.Obj;

            if (obj is ITickable updateable)
            {
                _updeablePool.Add(updateable);
            }

            if (obj is IStartable startable)
            {
                _startablePool.Add(startable);
            }

            if (obj is IInitializable awakeable)
            {
                _awakeablePool.Add(awakeable);
            }

            if (obj is ISubscribable subscribable)
            {
                _subscribablePool.Add(subscribable);
            }
        }

        private void AddToUpdateGameCycle<T>(T obj, bool b_lazy, bool b_transient)
        {
            if (obj == null) return;

            if (obj is IInitializable or IStartable)
            {
                if (b_transient)
                {
                    Debug.LogError(
                        $"Trying to add Transient class: \"{typeof(T)}\" to Update only game cycle, yet it implements some of full game cycle interfaces (they would be ignored) in Installer: \"{_installerType}\" on GameObject: \"{_bindedGameObject.name}\"");
                }
                else if (b_lazy)
                {
                    Debug.LogError(
                        $"Trying to add Lazy class: \"{typeof(T)}\" to Update only game cycle, yet it implements some of full game cycle interfaces (they would be ignored) in Installer: \"{_installerType}\" on GameObject: \"{_bindedGameObject.name}\"");
                }
            }
        }
    }
}