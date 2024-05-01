using UnityEngine;

namespace OCNContainer
{
    public interface IScope
    {
        public T Resolve<T>() where T : class;
        public T FindInHierarchy<T>() where T : Component;
        public T AddToLifecycle<T>() where T : class, new();
    }
}