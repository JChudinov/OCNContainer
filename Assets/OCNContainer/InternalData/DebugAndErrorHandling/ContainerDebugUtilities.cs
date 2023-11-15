using System;
using System.Collections.Generic;
using UnityEngine;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    public enum LoggingBypassMode
    {
        /// <summary>
        /// Used while message should only be shown while the first trouble encountered with this type
        /// </summary>
        FirstOnType,
        /// <summary>
        /// Used no matter if type is already registered or not
        /// </summary>
        AlwaysLog
    }
    
    public static class ContainerDebugUtilities
    {
        public static bool b_ValidationMode = false;
        //types of data with witch errors occured
        private static readonly List<Type> _defectiveTypes = new();
        
        private static string GetProperDebugData(Type installerType, GameObject bindedGameObject)
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

        public static void LogError(string message, ContainerDebugInfo containerDebugInfo, Type defectiveType, LoggingBypassMode loggingBypassMode)
        {
            if (_defectiveTypes.Contains(defectiveType))
            {
                if (loggingBypassMode == LoggingBypassMode.FirstOnType)
                {
                    return;
                }

                Debug.LogError(message + GetProperDebugData(containerDebugInfo.InstallerType, containerDebugInfo.BindedGameObject));
            }
            
            Debug.LogError(message);
            _defectiveTypes.Add(defectiveType);
        }
    }
}