using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    public static class ContainerValidation
    {
#if UNITY_EDITOR
        [MenuItem("Container/Validate and run _v")]
        private static void ValidateContainersAndRun()
        {
            if (Application.isPlaying) return;
            
            if (ValidateContainers())
            {
                EditorApplication.EnterPlaymode();
            }
        }
#endif

#if UNITY_EDITOR
        [MenuItem("Container/Validate #v")]
        private static void Validate()
        {
            if (Application.isPlaying) return;
            
            ValidateContainers();
        }
#endif
        
        private static bool ValidateContainers()
        {
            Debug.Log("Scene validation begin");

            ContainerDebugUtilities.b_ValidationMode = true;
            ContainerDebugUtilities.SubscribeClickableHyperlinks();

            var installers = Object.FindObjectsByType<Installer>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            var factorySpawnedInstallers = SpawnInstallersFromFactories();

            installers.AddRange(factorySpawnedInstallers);

            foreach (var installer in installers)
            {
                LifecycleManager.RegisterLifecycleParticipant(installer);
                LifecycleManager.RegisterForBindingPhaseParticipant(installer);
            }

            LifecycleManager.StartLifecycle();


            foreach (IInstallerResetable installer in installers)
            {
                installer.ResetInstaller();
            }

            foreach (var spawnedInstaller in factorySpawnedInstallers)
            {
                Object.DestroyImmediate(spawnedInstaller.gameObject);
            }

            ContainerDebugUtilities.b_ValidationMode = false;
            
            if (ContainerDebugUtilities.b_ValidationSucceeded)
            {
                Debug.Log("Validation succeeded");
                ContainerDebugUtilities.ResetDebugUtilitiesAfterValidation();

                return true;
            }
            else
            {
                Debug.LogError("Validation failed");
                ContainerDebugUtilities.ResetDebugUtilitiesAfterValidation();

                return false;
            }
        }
        
        private static List<Installer> SpawnInstallersFromFactories()
        {
            List<Type> factoryTypes = new();
            List<object> allFactoriesInScene = new();
            List<Installer> allInstallers = new();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var result =
                    assembly
                        .GetTypes()
                        .Where(t => t.BaseType != null && t.BaseType.IsGenericType &&
                                    t.BaseType.GetGenericTypeDefinition() == typeof(Factory<>)).ToList();

                factoryTypes.AddRange(result);
            }

            foreach (var factoryType in factoryTypes)
            {
                var factories = Object.FindObjectsByType(factoryType, FindObjectsInactive.Include, FindObjectsSortMode.None);

                allFactoriesInScene.AddRange(factories);
            }

            foreach (var factoryObject in allFactoriesInScene)
            {
                var memberInfo = factoryObject.GetType().BaseType;
                if (memberInfo != null)
                {
                    PropertyInfo field = memberInfo.GetProperty("InstallerPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (field == null) continue;
                    
                    var installerPrefab_unspecified = field.GetValue(factoryObject);

                    if (installerPrefab_unspecified is Installer installerPrefab)
                    {
                        var installer = Object.Instantiate(installerPrefab);
                        allInstallers.Add(installer);
                    }
                }
            }

            return allInstallers;
        }
    }
}