using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Car
{
    public class CarCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        [Header("Look")]
        [SerializeField] private float _sensitivity  = 2f;
        [SerializeField] private float _verticalMin  = -15f;
        [SerializeField] private float _verticalMax  =  45f;

        private CinemachineOrbitalFollow _orbital;

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            if (_vcamGameObject != null)
                _orbital = _vcamGameObject.GetComponent<CinemachineOrbitalFollow>();
        }

        public void HandleLook(Vector2 look)
        {
            if (_orbital == null) return;
            _orbital.HorizontalAxis.Value += look.x * _sensitivity;
            _orbital.VerticalAxis.Value    = Mathf.Clamp(
                _orbital.VerticalAxis.Value + look.y * _sensitivity,
                _verticalMin, _verticalMax);
        }

        public CameraRigHandle     GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => new CameraBlendSettings(0.4f, CameraBlendStyle.EaseInOut);
    }
}
