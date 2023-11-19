using System;
using System.Runtime.CompilerServices;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public partial class Installer
    {
        public static IScope CreateFromMethod(Action<IScopeRegistration> installer)
        {
            var container = new ContainerFromMethod(new GameObject("TestInstaller"), typeof(Installer), null, true, installer);

            LifecycleManager.RegisterLifecycleParticipant(container);
            LifecycleManager.RegisterForBindingPhaseParticipant(container);
            
            return container;
        }
    }
}

namespace OCNContainer.InternalData
{
    public class ContainerFromMethod : Container, IInstallBindingPhaseParticipant
    {
        private readonly Action<IScopeRegistration> _installBindingsCallback;

        public ContainerFromMethod(GameObject bindedGameObject, Type installerType, IParentContainerLookupable parentContainer, bool isCoreContainer,
            Action<IScopeRegistration> installBindingsCallback, Type facadeExpectedType = null) : base(bindedGameObject, installerType,
            parentContainer, isCoreContainer, facadeExpectedType)
        {
            _installBindingsCallback = installBindingsCallback;
        }

        public void InstallBindingsPhase()
        {
            _installBindingsCallback?.Invoke(this);
        }
    }
}