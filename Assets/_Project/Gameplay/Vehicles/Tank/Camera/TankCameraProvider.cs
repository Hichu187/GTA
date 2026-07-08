using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Tank
{
    public class TankCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        [Header("Look")]
        [SerializeField] private float _sensitivity    = 2f;
        [SerializeField] private float _verticalMin    = -10f;
        [SerializeField] private float _verticalMax    =  40f;

        [Header("Auto Reset")]
        [SerializeField] private float _resetDelay    = 1.5f;   // seconds before camera starts resetting
        [SerializeField] private float _resetSpeed    = 80f;    // deg/s for camera return
        [SerializeField] private float _defaultVertical = 15f;  // resting vertical angle

        private CinemachineOrbitalFollow _orbital;
        private float _lookHoldTimer;

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            if (_vcamGameObject != null)
                _orbital = _vcamGameObject.GetComponent<CinemachineOrbitalFollow>();
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

                    // "Behind tank" in world-space orbital = tankYaw angle
                    // DeltaAngle finds shortest path, handles wrap-around
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

        // Returns the direction the camera is currently aimed at.
        // Used by TankController to align the turret/barrel.
        public Vector3 GetAimDirection()
        {
            var cam = Camera.main;
            return cam != null ? cam.transform.forward : transform.forward;
        }

        public CameraRigHandle     GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => new CameraBlendSettings(0.4f, CameraBlendStyle.EaseInOut);
    }
}
