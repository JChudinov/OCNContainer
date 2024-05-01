using System;
using System.Collections.Generic;
using OCNContainer.InternalData.DebugAndErrorHandling;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public partial class Container
    {
        public event Action OnStartCycleComplete;

        private readonly List<IContainerLifecycleParticipant> _subContainers = new();
        private IParentContainerLookupable _parentContainer;

        void ILifecycleParticipant.InstanceCreationPhase()
        {
            foreach (var subContainer in _subContainers)
            {
                subContainer.InstanceCreationPhase();
            }

            foreach (var registrationData in _creationPhaseList)
            {
                registrationData.CreateObject();
                AddRegistrationDataToFullGameCycle(registrationData);
            }

            foreach (var subContainer in _subContainers)
            {
                AddRegistrationDataFromSubContainer(subContainer.FacadeRegistrationData);
            }

            if (_isCoreContainer == false && _facadeRegistrationData == null)
            {
                ContainerDebugUtilities.LogError(
                    $"No Facade of type {_facadeExpectedType.Name} found in SubContainer ",
                    DebugInfo,
                    _facadeExpectedType,
                    LoggingBypassMode.FirstOnType);
            }
        }

        void ILifecycleParticipant.ScopeResolvePhase()
        {
            foreach (var subContainer in _subContainers)
            {
                subContainer.ScopeResolvePhase();
            }

            foreach (var awakeable in _awakeablePool)
            {
                awakeable.OnInitialize(this);
            }
        }

        void ILifecycleParticipant.EventSubscriptionsPhase()
        {
            foreach (var subscribable in _subscribablePool)
            {
                subscribable.OnSubscription();
            }
        }

        void ILifecycleParticipant.StartPhase()
        {
            foreach (var startable in _startablePool)
            {
                startable.OnStart();
            }

            OnStartCycleComplete?.Invoke();
        }

        void ILifecycleParticipant.UpdatePhase()
        {
            foreach (var subContainer in _subContainers)
            {
                subContainer.UpdatePhase();
            }
            
            foreach (var updateable in _updeablePool)
            {
                updateable.OnTick();
            }
        }

        bool IParentContainerLookupable.TryFindRegistration<T>(out T foundObject)
        {
            foundObject =  Resolve_Internal<T>(true, DebugInfo.InstallerType);

            return foundObject != null;
        }
    }
}