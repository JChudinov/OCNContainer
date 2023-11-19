using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OCNContainer.InternalData.DebugAndErrorHandling
{
    public static class ContainerValidation
    {
        [MenuItem("Container/ValidateDependencies")]
        private static void ValidateContainers()
        {
            Debug.Log("Scene validation begin");

            ContainerDebugUtilities.b_ValidationMode = true;

            var installers = Object.FindObjectsByType<Installer>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            var factorySpawnedInstallers = SpawnInstallersFromFactories();

            installers.AddRange(factorySpawnedInstallers);

            foreach (var installer in installers)
            {
                LifecycleManager.RegisterLifecycleParticipant(installer);
                LifecycleManager.RegisterForBindingPhaseParticipant(installer);
            }

            LifecycleManager.StartLifecycle();

            if (ContainerDebugUtilities.b_ValidationSucceeded)
            {
                Debug.Log("Validation succeeded");
            }
            else
            {
                Debug.LogError("Validation failed");
            }


            foreach (IInstallerResetable installer in installers)
            {
                installer.ResetInstaller();
            }

            foreach (var spawnedInstaller in factorySpawnedInstallers)
            {
                Object.DestroyImmediate(spawnedInstaller.gameObject);
            }

            ContainerDebugUtilities.b_ValidationMode = false;
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
                    FieldInfo field = memberInfo.GetField("installerPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

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