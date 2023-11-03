using System;
using System.Collections.Generic;

namespace OCNContainer.InternalData
{
    public partial class Container : ILifecycleParticipant, IScopeRegistration
    {
        public event Action OnStartCycleComplete;

        private List<ILifecycleParticipant> _subContainers = new();
        
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