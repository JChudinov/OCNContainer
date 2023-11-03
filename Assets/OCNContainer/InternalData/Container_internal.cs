using System;
using System.Collections.Generic;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public partial class Container : ILifecycleParticipant, IScope, IScopeRegistration
    {
        private readonly Dictionary<Type, object> _registrationDataDictionary = new();
        private readonly Dictionary<Type, Type> _interfaceToTypeDictionary = new();

        private readonly List<RegistrationData> _creationPhaseList = new();
        private readonly List<ITickable> _updeablePool = new();
        private readonly List<IStartable> _startablePool = new();
        private readonly List<IInitializable> _awakeablePool = new();
        private readonly List<ISubscribable> _subscribablePool = new();

        #region Debug

        private string DebugInformationString => $"in Installer: \"{_installerType}\" on GameObject: \"{_bindedGameObject.name}\"";
        private GameObject _bindedGameObject;
        private Type _installerType;

        #endregion


        public Container(GameObject bindedGameObject, Type installerType)
        {
            _bindedGameObject = bindedGameObject;
            _installerType = installerType;

            foreach (var awakeable in _awakeablePool)
            {
                awakeable.Initialize(this);
            }
        }

        private void RegisterSubContainer_Internal(Action<IScopeRegistration> subContainer)
        {
            var newSubContainer = new Container(_bindedGameObject, _installerType);
            _subContainers.Add(newSubContainer);
            subContainer?.Invoke(newSubContainer);
        }

        private void Register_Internal<T>() where T : class, new()
        {
            if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                Debug.LogError(
                    $"Unable to Register subclass of MonoBehaviour: \"{typeof(T)}\" without providing and instance " + DebugInformationString);
                return;
            }

            var registrationData = RegistrationData.CreateFromImplementationAsSingle<T>();

            if (_registrationDataDictionary.TryAdd(typeof(T), registrationData))
            {
                _creationPhaseList.Add(registrationData);
            }
            else
            {
                Debug.LogError(
                    $"Type: {typeof(T).Name} already registered " + DebugInformationString);
            }
        }

        private void RegisterFromInstance_Internal<T>(T instance) where T : class
        {
            if (instance == null)
            {
                Debug.LogError($"Cant register null instance of type: \"{typeof(T).Name}\"" + DebugInformationString);
                return;
            }

            var registrationData = RegistrationData.CreateFromImplementationWithInstance<T>(instance);

            if (!_registrationDataDictionary.TryAdd(typeof(T), registrationData))
            {
                Debug.LogError(
                    $"Type: {typeof(T)} already registered " + DebugInformationString);
            }
        }

        private void Bind_Internal<TImplementation, TInterface>(bool b_firstInMultiBindIteration = false)
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
                if (b_firstInMultiBindIteration)
                {
                    var registrationData = RegistrationData.CreateFromInterfaceAsSingle<TImplementation>();

                    if (_registrationDataDictionary.TryAdd(typeof(TImplementation), registrationData))
                    {
                        _creationPhaseList.Add(registrationData);
                    }
                    else
                    {
                        Debug.LogError(
                            $"Type {typeof(TInterface)} is already registered while trying to bind interface " + DebugInformationString);
                    }
                }
            }
            else
            {
                Debug.LogError($"Interface {typeof(TInterface)} already registered " + DebugInformationString);
            }
        }

        private void BindFromInstance_Internal<TImplementation, TInterface>(TImplementation instance, bool b_firstInMultiBindIteration = false)
            where TImplementation : class, TInterface, new()
        {
            if (instance == null)
            {
                Debug.LogError($"Cant register null instance of type: \"{typeof(TImplementation)}\"" + DebugInformationString);
                return;
            }

            if (_interfaceToTypeDictionary.TryAdd(typeof(TInterface), typeof(TImplementation)))
            {
                if (b_firstInMultiBindIteration)
                {
                    var registrationData = RegistrationData.CreateFromInterfaceWithInstance<TImplementation>(instance);
                    if (_registrationDataDictionary.TryAdd(typeof(TImplementation), registrationData))
                    {
                        //_creationPhaseList.Add(registrationData);
                    }
                    else
                    {
                        Debug.LogError(
                            $"Type {typeof(TImplementation)} already registered " + DebugInformationString);
                    }
                }
            }
            else
            {
                Debug.LogError(
                    $"Type {typeof(TInterface)} already registered as interface " + DebugInformationString);
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
    }
}