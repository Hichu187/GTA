using UnityEngine;
using Unity.Cinemachine;
using Game.Core.Camera;

namespace Game.Gameplay.Character
{
    public class CharacterCameraProvider : MonoBehaviour, ICameraContextProvider
    {
        public enum CameraMode { ThirdPerson, FirstPerson }

        [SerializeField] private GameObject _thirdPersonVcam;
        [SerializeField] private GameObject _firstPersonVcam;
        [SerializeField] private CameraMode _defaultMode = CameraMode.ThirdPerson;

        [Header("Look Sensitivity")]
        [SerializeField] private float _sensitivity = 2f;

        private CameraMode               _current;
        private CinemachineOrbitalFollow _tpOrbital;
        private CinemachinePanTilt       _fpPanTilt;
        private float                    _pendingFPYaw;

        public CameraMode CurrentMode => _current;

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            _current = _defaultMode;

            if (_thirdPersonVcam != null)
                _tpOrbital = _thirdPersonVcam.GetComponent<CinemachineOrbitalFollow>();

            if (_firstPersonVcam != null)
                _fpPanTilt = _firstPersonVcam.GetComponent<CinemachinePanTilt>();
        }

        // Called by Character.Update() each frame while possessed.
        public void HandleLook(Vector2 lookAxis)
        {
            if (_current == CameraMode.ThirdPerson && _tpOrbital != null)
            {
                _tpOrbital.HorizontalAxis.Value += lookAxis.x * _sensitivity;
                _tpOrbital.VerticalAxis.Value   += lookAxis.y * _sensitivity;
            }
            else if (_current == CameraMode.FirstPerson && _fpPanTilt != null)
            {
                // Horizontal pan is baked into Character body rotation (see ConsumeFPBodyYawDelta).
                // PanTilt only handles vertical tilt to avoid double-rotation with the body.
                _pendingFPYaw         = lookAxis.x * _sensitivity;
                _fpPanTilt.PanAxis.Value  = 0f;
                _fpPanTilt.TiltAxis.Value -= lookAxis.y * _sensitivity;
            }
        }

        /// <summary>
        /// Returns horizontal yaw delta (degrees) for Character to apply to its body in FP mode.
        /// Must be consumed every frame — returns 0 in TP mode.
        /// </summary>
        public float ConsumeFPBodyYawDelta()
        {
            var delta = _pendingFPYaw;
            _pendingFPYaw = 0f;
            return delta;
        }

        public void Toggle()
        {
            _current = _current == CameraMode.ThirdPerson
                ? CameraMode.FirstPerson
                : CameraMode.ThirdPerson;

            // Entering FP: reset PanAxis so body rotation alone owns horizontal look.
            if (_current == CameraMode.FirstPerson && _fpPanTilt != null)
                _fpPanTilt.PanAxis.Value = 0f;

            CameraRigChanged?.Invoke();
        }

        public CameraRigHandle    GetActiveCameraRig() =>
            new CameraRigHandle(_current == CameraMode.ThirdPerson ? _thirdPersonVcam : _firstPersonVcam);

        public CameraBlendSettings GetBlendSettings() => CameraBlendSettings.Cut;
    }
}
