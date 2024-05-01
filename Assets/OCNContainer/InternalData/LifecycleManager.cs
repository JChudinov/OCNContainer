using System;
using System.Collections.Generic;
using OCNContainer.InternalData.DebugAndErrorHandling;
using UnityEditor;
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

            if (ContainerDebugUtilities.b_ValidationMode == false)
            {
                foreach (var containerLifecycle in _participants)
                {
                    containerLifecycle.EventSubscriptionsPhase();
                }

                foreach (var containerLifecycle in _participants)
                {
                    containerLifecycle.StartPhase();
                }
            }

            _participants.Clear();

            //TODO: not sure if it should only be cleared in validation mode or not
            if (ContainerDebugUtilities.b_ValidationMode)
            {
                _bindingPhaseParticipants.Clear();
            }
        }

        //Unity aint clear static on exit play mode, so have to do it manually, not to waste time reloading Domain
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PlayStateNotifier()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += ModeChanged;
#endif
        }
        
#if UNITY_EDITOR
        private static void ModeChanged(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllStaticData();
            }
        }
        
        private static void ClearAllStaticData()
        {
            _bindingPhaseParticipants.Clear();
            _participants.Clear();
        }
#endif
    }
}