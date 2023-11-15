using System;
using UnityEngine;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    public class ContainerDebugInfo
    {
        public Type InstallerType { get; }
        public GameObject BindedGameObject { get; }

        public ContainerDebugInfo(Type installerType, GameObject bindedGameObject)
        {
            InstallerType = installerType;
            BindedGameObject = bindedGameObject;
        }
    }
}