
using System;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    /// <summary>
    /// Base class for module entry point
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public abstract partial class Installer : MonoBehaviour
    {
        public event Action OnInstallationComplete;
        
        protected abstract void InstallBindings(IScopeRegistration container);
    }
}