using System;
using UnityEngine;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    public static class ContainerDebugUtilities
    {
        public static string GetProperDebugData(Type installerType, GameObject bindedGameObject)
        {
            string properInstallerNameHandling = string.Empty;
            
#if UNITY_EDITOR
            properInstallerNameHandling = $"<a href=\"{installerType.Name}\">{installerType.Name}</a>";
#else
            properInstallerNameHandling = installerType.Name;
#endif
            var str = $"in Installer: \"{properInstallerNameHandling}\" on GameObject: \"{bindedGameObject.name}\"";
            return str;
        }
    }
}