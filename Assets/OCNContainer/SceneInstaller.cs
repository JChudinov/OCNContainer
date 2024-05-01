
using System;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public abstract class SceneInstaller : Installer
    {
        private static SceneInstaller Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                
                LifecycleManager.RegisterLifecycleParticipant(this);
                LifecycleManager.RegisterForBindingPhaseParticipant(this);
            }
        }
    }
}