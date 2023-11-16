using UnityEngine;

namespace OCNContainer
{
    public class Factory<T> : MonoBehaviour where T : class
    {
        [SerializeField] private T installerPrefab;

        public T Create()
        {
            if (installerPrefab is not Installer)
            {
                Debug.LogError("prefab is not Installer");
            }

            return (Instantiate(installerPrefab as Installer) as T);
        }
    }
}