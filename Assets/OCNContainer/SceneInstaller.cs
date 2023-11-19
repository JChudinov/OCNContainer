
using System;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public abstract class SceneInstaller : MonoBehaviour
    {
        private Container _sceneContainer;

        private static SceneInstaller Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        protected abstract void SceneSetup();
    }
}