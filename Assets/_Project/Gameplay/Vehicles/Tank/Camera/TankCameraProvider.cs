using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Tank
{
    public class TankCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        [Header("Look")]
        [SerializeField] private float _sensitivity     = 2f;
        [SerializeField] private float _verticalMin     = -10f;
        [SerializeField] private float _verticalMax     =  40f;

        [Header("Auto Reset")]
        [SerializeField] private float _resetDelay      = 1.5f;
        [SerializeField] private float _resetSpeed      = 80f;
        [SerializeField] private float _defaultVertical = 15f;

        [Header("Fire Shake")]
        [SerializeField] private float _shakeAmplitude  = 3f;    // degrees of vertical kick
        [SerializeField] private float _shakeDuration   = 0.35f; // seconds
        [SerializeField] private float _shakeFrequency  = 18f;   // oscillations per second

        private CinemachineOrbitalFollow _orbital;
        private float _lookHoldTimer;

        // Manual shake state
        private float _shakeTimer;
        private float _prevShakeOffset; // delta applied last frame — subtracted next frame

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            if (_vcamGameObject != null)
                _orbital = _vcamGameObject.GetComponent<CinemachineOrbitalFollow>();
        }

        public void TriggerFireShake()
        {
            _shakeTimer = _shakeDuration;
        }

        private void Update()
        {
            if (_orbital == null) return;

            // Undo last frame's shake so the base axis value stays clean
            _orbital.VerticalAxis.Value -= _prevShakeOffset;

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(_shakeTimer / _shakeDuration); // 1 → 0
                _prevShakeOffset = Mathf.Sin(Time.time * _shakeFrequency) * _shakeAmplitude * t;
                _orbital.VerticalAxis.Value = Mathf.Clamp(
                    _orbital.VerticalAxis.Value + _prevShakeOffset,
                    _verticalMin, _verticalMax);
            }
            else
            {
                _prevShakeOffset = 0f;
            }
        }

        public void HandleLook(Vector2 look)
        {
            if (_orbital == null) return;

            if (look.sqrMagnitude > 0.001f)
            {
                _orbital.HorizontalAxis.Value += look.x * _sensitivity;
                _orbital.VerticalAxis.Value    = Mathf.Clamp(
                    _orbital.VerticalAxis.Value + look.y * _sensitivity,
                    _verticalMin, _verticalMax);
                _lookHoldTimer = _resetDelay;
            }
            else
            {
                _lookHoldTimer -= Time.deltaTime;
                if (_lookHoldTimer <= 0f)
                {
                    float step = _resetSpeed * Time.deltaTime;

                    float behindH = transform.eulerAngles.y;
                    float deltaH  = Mathf.DeltaAngle(_orbital.HorizontalAxis.Value, behindH);
                    _orbital.HorizontalAxis.Value = Mathf.MoveTowards(
                        _orbital.HorizontalAxis.Value,
                        _orbital.HorizontalAxis.Value + deltaH,
                        step);

                    _orbital.VerticalAxis.Value = Mathf.MoveTowards(
                        _orbital.VerticalAxis.Value, _defaultVertical, step);
                }
            }
        }

        public Vector3 GetAimDirection()
        {
            var cam = Camera.main;
            return cam != null ? cam.transform.forward : transform.forward;
        }

        public CameraRigHandle     GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => new CameraBlendSettings(0.4f, CameraBlendStyle.EaseInOut);
    }
}
