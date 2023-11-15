using UnityEditor;
using UnityEngine;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    public static class ContainerValidation
    {
        [MenuItem("Container/ValidateDependencies")]
        private static void ValidateContainers()
        {
            Debug.Log("container validation begin");
            var a = Object.FindObjectsByType<Installer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            Debug.Log("installers count " + a.Length);
        }
    }
}