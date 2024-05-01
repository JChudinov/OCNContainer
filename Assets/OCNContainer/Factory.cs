using UnityEngine;

namespace OCNContainer
{
    public class Factory<T> : MonoBehaviour where T : class
    {
        [field: SerializeField] protected T InstallerPrefab { get; private set; }
    }
}