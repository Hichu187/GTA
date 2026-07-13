using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public class HelicopterCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        [SerializeField] private GameObject _vcamGameObject;

        [Header("Look")]
        [SerializeField] private float _sensitivity = 2f;
        [SerializeField] private float _returnSpeed = 2f;
        [SerializeField] private float _returnDelay = 0.3f;
        [SerializeField] private float _verticalMin = -20f;
        [SerializeField] private float _verticalMax =  60f;

        private CinemachineOrbitalFollow _orbital;
        private float                    _lastInputTime;

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            if (_vcamGameObject != null)
                _orbital = _vcamGameObject.GetComponent<CinemachineOrbitalFollow>();
        }

        // isManualTurning: true while the player holds Yaw/A-D input (pure body rotation).
        // While true, the camera does not recenter behind the nose — it only resumes
        // once the player stops turning, so spinning in place doesn't drag the camera along.
        public void HandleLook(Vector2 look, bool isManualTurning = false)
        {
            if (_orbital == null) return;
            bool hasInput = look.sqrMagnitude > 0.01f;
            if (hasInput)
            {
                _lastInputTime = Time.time;
                _orbital.HorizontalAxis.Value += look.x * _sensitivity;
                _orbital.VerticalAxis.Value    = Mathf.Clamp(
                    _orbital.VerticalAxis.Value + look.y * _sensitivity,
                    _verticalMin, _verticalMax);
            }
            else if (!isManualTurning && Time.time - _lastInputTime >= _returnDelay)
            {
                _orbital.HorizontalAxis.Value = Mathf.LerpAngle(
                    _orbital.HorizontalAxis.Value,
                    transform.eulerAngles.y,
                    _returnSpeed * Time.deltaTime);
            }
        }

        public CameraRigHandle     GetActiveCameraRig() => new CameraRigHandle(_vcamGameObject);
        public CameraBlendSettings GetBlendSettings()   => new CameraBlendSettings(0.4f, CameraBlendStyle.EaseInOut);
    }
}
