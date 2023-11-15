using OCNContainer;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public abstract partial class Installer : MonoBehaviour, ILifecycleParticipant, IInstallBindingPhaseParticipant, IParentContainerLookupable
    {
        private Container _container;
        private IParentContainerLookupable _parentContainerLookupableImplementation;

        private IScopeRegistration Container_internal
        {
            get
            {
                if (_container != null)
                {
                    return _container;
                }
                else
                {
                    _container = new Container(gameObject, this.GetType(), this, true);
                    return _container;
                }
            }
        }
        
        private void Awake()
        {
            LifecycleManager.RegisterLifecycleParticipant(this);
            LifecycleManager.RegisterForBindingPhaseParticipant(this);

            
            
        }

        private void Start()
        {
            if (LifecycleManager.b_EngineStartPhaseBegin == false)
            {
                LifecycleManager.StartLifecycle();
            }
        }

        private void Update()
        {
            if (Container == null)
            {
                Debug.LogError("Container is null");
                return;
            }

            (_container as ILifecycleParticipant).UpdatePhase();
        }

        

        void ILifecycleParticipant.InstanceCreationPhase()
        {
            (_container as ILifecycleParticipant).InstanceCreationPhase();
        }

        void ILifecycleParticipant.ScopeResolvePhase()
        {
            (_container as ILifecycleParticipant).ScopeResolvePhase();   
        }

        void ILifecycleParticipant.EventSubscriptionsPhase()
        {
            (_container as ILifecycleParticipant).EventSubscriptionsPhase();
        }

        void ILifecycleParticipant.StartPhase()
        {
            (_container as ILifecycleParticipant).StartPhase();
        }

        void ILifecycleParticipant.UpdatePhase()
        {
            //TODO: probably create single call from LifecycleManager, if not => create another Interface to inherit full lifecycle with update
        }

        void IInstallBindingPhaseParticipant.InstallBindingsPhase()
        {
            if (_container == null)
            {
                InstallBindings(Container);
                
            }
            //Not null after calling property
            // ReSharper disable once PossibleNullReferenceException
            _container.OnStartCycleComplete += () =>
            {
                OnInstallationComplete?.Invoke();
            };
        }

        public bool TryFindRegistration<T>(out T foundObject) where T : class
        {
            foundObject = null;
            return _parentContainerLookupableImplementation != null && _parentContainerLookupableImplementation.TryFindRegistration(out foundObject);
        }
    }
}
