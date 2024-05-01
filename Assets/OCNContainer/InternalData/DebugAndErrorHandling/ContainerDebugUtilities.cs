using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        public static bool b_ValidationSucceeded = true;

        //types of data with witch errors occured
        private static readonly List<Type> _defectiveTypes = new();
        private static bool b_SubscribedForHyperlinks;

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
            if (b_ValidationMode) b_ValidationSucceeded = false;

            if (_defectiveTypes.Contains(defectiveType))
            {
                if (loggingBypassMode == LoggingBypassMode.FirstOnType)
                {
                    return;
                }

                Debug.LogError(message + GetProperDebugData(containerDebugInfo.InstallerType, containerDebugInfo.BindedGameObject));
            }

            Debug.LogError(message + GetProperDebugData(containerDebugInfo.InstallerType, containerDebugInfo.BindedGameObject));
            _defectiveTypes.Add(defectiveType);
        }

        public static void ResetDebugUtilitiesAfterValidation()
        {
            _defectiveTypes.Clear();
            b_ValidationSucceeded = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void SubscribeClickableHyperlinks()
        {
#if UNITY_EDITOR
            EditorGUI.hyperLinkClicked += OnHyperlinkClicked;
#endif
        }

        public static void UnsubscribeClickableHyperlinks()
        {
#if UNITY_EDITOR
            if (b_SubscribedForHyperlinks == false)
            {
                b_SubscribedForHyperlinks = true;
                EditorGUI.hyperLinkClicked -= OnHyperlinkClicked;
            }
#endif
        }

#if  UNITY_EDITOR
        private static void OnHyperlinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            string lPath = args.hyperLinkData["href"] + ".cs";

            foreach (var lAssetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (lAssetPath.EndsWith(lPath))
                {
                    var lScript = (MonoScript)AssetDatabase.LoadAssetAtPath(lAssetPath, typeof(MonoScript));
                    if (lScript != null)
                    {
                        AssetDatabase.OpenAsset(lScript);
                        break;
                    }
                }
            }
        } 
#endif
        
        public static string LogResolveFailedStackTrace()
        {
            string debugData = String.Empty;
            StackTrace stackTrace = new StackTrace();
            
            foreach (var frame in stackTrace.GetFrames())
            {
                Debug.Log(frame.GetMethod().Name);
                if(frame.GetMethod().Name is "OnInitialize" or "OCNContainer.IInitializable.OnInitialize" )
                {
                    debugData = $" in class: \"<a href=\"{frame.GetMethod().ReflectedType.Name}\">{frame.GetMethod().ReflectedType.Name}</a>\" ";
                    //debugData = $" in class: \"<a href=\"D:\\dev\\UnityProjects\\OCNContainer\\Assets\\_Content\\Scripts\\SubContainerTest\\SubContainerTest.cs\">{frame.GetMethod().ReflectedType.Name}</a>\" ";
                }
            }
            

            return debugData;
        }
    }
}