
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public abstract class SceneInstaller : MonoBehaviour
    {
        private Container _sceneContainer;

        protected abstract void SceneSetup();
    }
}