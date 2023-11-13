using System;
using System.Collections.Generic;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public partial class Container
    {
        public event Action OnStartCycleComplete;

        private readonly List<IContainerLifecycleParticipant> _subContainers = new();
        
        void ILifecycleParticipant.InstanceCreationPhase()
        {
            foreach (var subContainer in _subContainers)
            {
                subContainer.InstanceCreationPhase();
            }
            
            foreach (var registrationData in _creationPhaseList)
            {
                registrationData.CreateObject();
                AddToFullGameCycle(registrationData);
            }

            foreach (var subContainer in _subContainers)
            {
                AddRegistrationDataFromSubContainer(subContainer.FacadeRegistrationData);
            }

            if (_isCoreContainer == false && _facadeRegistrationData == null)
            {
                Debug.LogError($"No Facade of type {_facadeExpectedType.Name} found in SubContainer " + DebugInformationString);
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
                awakeable.Initialize(this);
            }
        }

        void ILifecycleParticipant.EventSubscriptionsPhase()
        {
            foreach (var subscribable in _subscribablePool)
            {
                subscribable.EventSubscriptions();
            }
        }

        void ILifecycleParticipant.StartPhase()
        {
            foreach (var startable in _startablePool)
            {
                startable.Start();
            }
            
            OnStartCycleComplete?.Invoke();
        }

        public void UpdatePhase()
        {
            foreach (var updateable in _updeablePool)
            {
                updateable.Tick();
            }
        }
    }

    
}