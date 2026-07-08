using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Airplane
{
    public class AirplaneCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        [Header("Look")]
        [SerializeField] private float _sensitivity    = 2f;
        [SerializeField] private float _verticalMin    = -20f;
        [SerializeField] private float _verticalMax    =  60f;

        [Header("Auto-Return")]
        [Tooltip("Seconds of no look input before camera starts returning behind.")]
        [SerializeField] private float _returnDelay    = 1.0f;
        [Tooltip("Lerp speed for returning to behind position.")]
        [SerializeField] private float _returnSpeed    = 2.5f;
        [Tooltip("Default vertical angle when camera is auto-returned.")]
        [SerializeField] private float _defaultVertical = 20f;

        private CinemachineOrbitalFollow _orbital;
        private float _noInputTimer;

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            if (_vcamGameObject != null)
                _orbital = _vcamGameObject.GetComponent<CinemachineOrbitalFollow>();
        }

        public void HandleLook(Vector2 look)
        {
            if (_orbital == null) return;

            if (look.sqrMagnitude > 0.0001f)
            {
                _orbital.HorizontalAxis.Value += look.x * _sensitivity;
                _orbital.VerticalAxis.Value    = Mathf.Clamp(
                    _orbital.VerticalAxis.Value + look.y * _sensitivity,
                    _verticalMin, _verticalMax);
                _noInputTimer = 0f;
                return;
            }

            _noInputTimer += Time.deltaTime;
            if (_noInputTimer < _returnDelay) return;

            float t = _returnSpeed * Time.deltaTime;
            _orbital.HorizontalAxis.Value = Mathf.LerpAngle(_orbital.HorizontalAxis.Value, 0f, t);
            _orbital.VerticalAxis.Value   = Mathf.Lerp(_orbital.VerticalAxis.Value, _defaultVertical, t);
        }

        public CameraRigHandle     GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => new CameraBlendSettings(0.5f, CameraBlendStyle.EaseInOut);
    }
}
