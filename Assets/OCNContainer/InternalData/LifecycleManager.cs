using System.Collections.Generic;
using UnityEngine;

namespace OCNContainer.InternalData
{
    public static class LifecycleManager
    {
        public static bool b_EngineStartPhaseBegin { get; private set; }

        private static readonly List<IInstallBindingPhaseParticipant> _bindingPhaseParticipants = new();

        private static readonly List<ILifecycleParticipant> _participants = new();

        public static void RegisterForBindingPhaseParticipant(IInstallBindingPhaseParticipant bindingPhaseParticipant)
        {
            _bindingPhaseParticipants.Add(bindingPhaseParticipant);
            
            if (b_EngineStartPhaseBegin) b_EngineStartPhaseBegin = false;
        }
        
        public static void RegisterLifecycleParticipant(ILifecycleParticipant participant)
        {
            _participants.Add(participant);

            if (b_EngineStartPhaseBegin) b_EngineStartPhaseBegin = false;
        }

        public static void ExcludeParticipant(ILifecycleParticipant participant)
        {
            _participants.Remove(participant);
        }
        
        public static void StartLifecycle()
        {
            b_EngineStartPhaseBegin = true;

            foreach (var bindingPhaseParticipant in _bindingPhaseParticipants)
            {
                bindingPhaseParticipant.InstallBindingsPhase();
            }

            foreach (var containerLifecycle in _participants)
            {
                containerLifecycle.InstanceCreationPhase();   
            }

            foreach (var containerLifecycle in _participants)
            {
                containerLifecycle.ScopeResolvePhase();
            }

            foreach (var containerLifecycle in _participants)
            {
                containerLifecycle.EventSubscriptionsPhase();
            }
            
            foreach (var containerLifecycle in _participants)
            {
                containerLifecycle.StartPhase();
            }
            
            _participants.Clear();
            
        }
    }

    
}