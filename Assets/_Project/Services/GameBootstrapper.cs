using UnityEngine;
using Game.Core.Possession;
using Game.Systems.Camera;

namespace Game.Services
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameplayServiceLocator _serviceLocator;
        [SerializeField] private CameraManager         _cameraManager;

        // Assigned in Inspector as a MonoBehaviour that implements IPossessable.
        [SerializeField] private MonoBehaviour _initialPossessable;

        private void Start()
        {
            _cameraManager.FindAndRegisterAllCameras();

            if (_initialPossessable != null &&
                _initialPossessable.TryGetComponent(out IPossessable possessable))
            {
                _serviceLocator.PossessionManager.Possess(possessable);
            }
            else
            {
                UnityEngine.Debug.LogError(
                    $"[GameBootstrapper] '{_initialPossessable?.gameObject.name}' has no IPossessable component.", this);
            }
        }
    }
}
