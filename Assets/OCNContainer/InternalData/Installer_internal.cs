using OCNContainer;
using OCNContainer.InternalData;
using UnityEngine;

namespace OCNContainer
{
    public abstract partial class Installer : MonoBehaviour, ILifecycleParticipant, IInstallBindingPhaseParticipant
    {
        private Container _container;

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
                    _container = new Container(gameObject, this.GetType(), true);
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

            _container.UpdatePhase();
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
    }
}
