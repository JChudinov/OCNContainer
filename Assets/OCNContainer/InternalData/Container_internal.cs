using System;
using System.Collections.Generic;
using OCNContainer.InternalData;
using OCNContainer.InternalData.DebugAndErrorHandling;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public partial class Container : IContainerLifecycleParticipant, IScope, IScopeRegistration, IFacadeSettable, IParentContainerLookupable
    {
        private readonly Dictionary<Type, object> _registrationDataDictionary = new();
        private readonly Dictionary<Type, Type> _interfaceToTypeDictionary = new();

        private readonly List<RegistrationData> _creationPhaseList = new();
        private readonly List<ITickable> _updeablePool = new();
        private readonly List<IStartable> _startablePool = new();
        private readonly List<IInitializable> _awakeablePool = new();
        private readonly List<ISubscribable> _subscribablePool = new();

        private RegistrationData _facadeRegistrationData;
        RegistrationData IContainerLifecycleParticipant.FacadeRegistrationData => _facadeRegistrationData;

        //for SubContainers only
        private Type _facadeExpectedType;


        #region Debug

        private ContainerDebugInfo DebugInfo { get; }

        //$"in Installer: \"{_installerType.Name}\" on GameObject: \"{_bindedGameObject.name}\"";
        private bool _isCoreContainer;

        #endregion


        public Container(GameObject bindedGameObject, Type installerType, IParentContainerLookupable parentContainer, bool isCoreContainer, Type facadeExpectedType = null)
        {
            _parentContainer = parentContainer;
            _isCoreContainer = isCoreContainer;
            _facadeExpectedType = facadeExpectedType;
            DebugInfo = new ContainerDebugInfo(installerType, bindedGameObject);

            foreach (var awakeable in _awakeablePool)
            {
                awakeable.Initialize(this);
            }
        }

        void IFacadeSettable.SetFacade(RegistrationData registrationData)
        {
            if (_facadeRegistrationData != null)
            {
                ContainerDebugUtilities.LogError(
                    "Multiple Facade registrations found ",
                    DebugInfo,
                    registrationData.CurrentType,
                    LoggingBypassMode.AlwaysLog);
                return;
            }

            if (_facadeExpectedType != null)
            {
                if (_facadeExpectedType == registrationData.CurrentType)
                {
                    _facadeRegistrationData = registrationData;
                }
                else
                {
                    ContainerDebugUtilities.LogError(
                        $"Facade type \"{registrationData.CurrentType.Name}\" " +
                        $"doesn't match Facade expected type \"{_facadeExpectedType.Name}\" ",
                        DebugInfo,
                        registrationData.CurrentType,
                        LoggingBypassMode.AlwaysLog);
                }
            }
        }

        private void RegisterSubContainer_Internal<T>(Action<IScopeRegistration> subContainer)
        {
            var newSubContainer = new Container(DebugInfo.BindedGameObject, DebugInfo.InstallerType, this, false, typeof(T));
            _subContainers.Add(newSubContainer);
            subContainer?.Invoke(newSubContainer);
        }

        private IRegistrationBuilder Register_Internal<T>() where T : class, new()
        {
            if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                ContainerDebugUtilities.LogError(
                    $"Unable to Register subclass of MonoBehaviour: \"{typeof(T)}\" without providing and instance ",
                    DebugInfo,
                    typeof(T),
                    LoggingBypassMode.AlwaysLog);
                return new RegistrationBuilderNullHandler();
            }

            var registrationData = RegistrationData.CreateFromImplementationAsSingle<T>(this);

            if (_registrationDataDictionary.TryAdd(typeof(T), registrationData))
            {
                _creationPhaseList.Add(registrationData);
                return registrationData;
            }
            else
            {
                ContainerDebugUtilities.LogError(
                    $"Type: {typeof(T).Name} already registered ",
                    DebugInfo,
                    typeof(T),
                    LoggingBypassMode.AlwaysLog);
                return new RegistrationBuilderNullHandler();
            }
        }

        private void RegisterFromInstance_Internal<T>(T instance) where T : class
        {
            if (instance == null)
            {
                ContainerDebugUtilities.LogError(
                    $"Cant register null instance of type: \"{typeof(T).Name}\" ",
                    DebugInfo,
                    typeof(T),
                    LoggingBypassMode.AlwaysLog);
                return;
            }

            var registrationData = RegistrationData.CreateFromImplementationWithInstance<T>(this, instance);

            if (!_registrationDataDictionary.TryAdd(typeof(T), registrationData))
            {
                ContainerDebugUtilities.LogError(
                    $"Type: {typeof(T)} already registered ",
                    DebugInfo,
                    typeof(T),
                    LoggingBypassMode.AlwaysLog);
            }
        }

        private void Bind_Internal<TImplementation, TInterface>(bool b_firstInMultiBindIteration = false)
            where TImplementation : class, TInterface, new()
        {
            if (typeof(TImplementation).IsSubclassOf(typeof(MonoBehaviour)))
            {
                ContainerDebugUtilities.LogError(
                    $"Unable to Register subclass of MonoBehaviour: \"{typeof(TImplementation)}\" without providing and instance",
                    DebugInfo,
                    typeof(TImplementation),
                    LoggingBypassMode.AlwaysLog);
                return;
            }


            if (_interfaceToTypeDictionary.TryAdd(typeof(TInterface), typeof(TImplementation)))
            {
                if (b_firstInMultiBindIteration)
                {
                    var registrationData = RegistrationData.CreateFromInterfaceAsSingle<TImplementation>(this);

                    if (_registrationDataDictionary.TryAdd(typeof(TImplementation), registrationData))
                    {
                        _creationPhaseList.Add(registrationData);
                    }
                    else
                    {
                        ContainerDebugUtilities.LogError(
                            $"Type \"{typeof(TImplementation)}\" is already registered while trying to bind interface ",
                            DebugInfo,
                            typeof(TImplementation),
                            LoggingBypassMode.AlwaysLog);
                    }
                }
            }
            else
            {
                ContainerDebugUtilities.LogError(
                    $"Interface \"{typeof(TInterface)}\" already registered ",
                    DebugInfo,
                    typeof(TInterface),
                    LoggingBypassMode.AlwaysLog);
            }
        }

        private void BindFromInstance_Internal<TImplementation, TInterface>(TImplementation instance, bool b_firstInMultiBindIteration = false)
            where TImplementation : class, TInterface, new()
        {
            if (instance == null)
            {
                ContainerDebugUtilities.LogError(
                    $"Cant register null instance of type: \"{typeof(TImplementation)}\" ",
                    DebugInfo,
                    typeof(TImplementation),
                    LoggingBypassMode.AlwaysLog);
                return;
            }

            if (_interfaceToTypeDictionary.TryAdd(typeof(TInterface), typeof(TImplementation)))
            {
                if (b_firstInMultiBindIteration)
                {
                    var registrationData = RegistrationData.CreateFromInterfaceWithInstance<TImplementation>(this, instance);
                    if (_registrationDataDictionary.TryAdd(typeof(TImplementation), registrationData))
                    {
                        //_creationPhaseList.Add(registrationData);
                    }
                    else
                    {
                        Debug.LogError($"Type {typeof(TImplementation)} already registered " + DebugInfo);
                    }
                }
            }
            else
            {
                Debug.LogError($"Type {typeof(TInterface)} already registered as interface " + DebugInfo);
            }
        }


        private T Resolve_Internal<T>(bool b_fromInternalContainerLookup = false, Type requesterType = null) where T : class
        {
            Type typeToResolve = typeof(T);
            //TODO: revert it to "resolvedFromImplementation"
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
                    //Searching Type was found found in a collection of concrete types, but it was registered as an interface in container
                    if (resolvedFromInterface == false && registrationData.RegisteredFromImplementation == false)
                    {
                        if (requesterType == null)
                        {
                            Debug.LogError($"No Requester Type found while trying to search \"{typeToResolve}\"");
                            return null;
                        }
                        
                        if (b_fromInternalContainerLookup)
                        {
                            
                            ContainerDebugUtilities.LogError(
                                $"Cant resolve concrete type \" {typeof(T)}\"" +
                                $" while resolving \"{requesterType}\" as it was registered as interface ",
                                DebugInfo,
                                typeToResolve,
                                LoggingBypassMode.FirstOnType);
                        }
                        else
                        {
                            ContainerDebugUtilities.LogError(
                                $"Cant resolve concrete type \" {typeof(T)}\" as it was registered as interface ",
                                DebugInfo,
                                typeof(T),
                                LoggingBypassMode.FirstOnType);
                        }
                        return null;
                    }

                    if (resolvedFromInterface == true && registrationData.RegisteredFromImplementation == true)
                    {
                        //TODO: could this even happen?
                        Debug.LogError(
                            $"Interface: \"{typeof(T)}\" is not associated in container with type: \"{typeToResolve}\" " +
                            DebugInfo);
                        return null;
                    }

                    return registrationData.Obj as T;
                }
                else
                {
                    ContainerDebugUtilities.LogError(
                        $"Instance of type \"{typeToResolve}\" was not registered correctly ",
                        DebugInfo,
                        typeToResolve,
                        LoggingBypassMode.AlwaysLog);
                    return null;
                }
            }
            // there is 2 possible ways to come here: from user Resolve and from child container lookup. In 1st scenario we would try to search
            //dependency in parent containers and if no dep found -> Log error. 
            else
            {
                if (_parentContainer != null &&  _parentContainer.TryFindRegistration(out T foundObject))
                {
                    return foundObject;
                }
                else
                {
                    if (b_fromInternalContainerLookup == false)
                    {
                        ContainerDebugUtilities.LogError(
                            $"Unable to retrieve proper registration data related for type: \"{typeToResolve.Name}\" ",
                            DebugInfo,
                            typeToResolve,
                            LoggingBypassMode.FirstOnType);
                    }
                    return null;
                }
            }
        }

//TODO:
//perhaps, both return statements should be logged as different errors
        private bool TryFindRegistrationData(Type type, out RegistrationData validRegistration)
        {
            if (_registrationDataDictionary.TryGetValue(type, out var registrationData_unspecified))
            {
                if (registrationData_unspecified is not RegistrationData registrationData)
                {
                    validRegistration = null;
                    return false;
                }

                validRegistration = registrationData;
                return true;
            }

            validRegistration = null;
            return false;
        }

        private void AddToFullGameCycle(RegistrationData registration)
        {
            if (registration.IsLazy && registration.Obj is IInitializable or IStartable)
            {
                Debug.LogError("Trying to add Lazy binding of class ");
            }

            var obj = registration.Obj;

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

        private void AddRegistrationDataFromSubContainer(RegistrationData facadeRegistrationData)
        {
            if (facadeRegistrationData == null)
            {
                //TODO: perhaps should log it somehow, but cant message to LogUtilities due to no proper type in null RegistrationData
                //Debug.LogError($"Cant register null instance of facade from subContainer " + DebugInformationString);
                return;
            }

            if (!_registrationDataDictionary.TryAdd(facadeRegistrationData.CurrentType, facadeRegistrationData))
            {
                ContainerDebugUtilities.LogError(
                    $"Type: {facadeRegistrationData.CurrentType.Name} already registered ",
                    DebugInfo,
                    facadeRegistrationData.CurrentType,
                    LoggingBypassMode.AlwaysLog);
            }
        }
    }
}