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
        [SerializeField] private GameObject _aimVcam;
        [SerializeField] private CameraMode _defaultMode = CameraMode.ThirdPerson;

        [Header("Look Sensitivity")]
        [SerializeField] private float _sensitivity   = 2f;
        [SerializeField] private float _returnSpeed   = 2f;
        [SerializeField] private float _returnDelay   = 0.3f;

        private CameraMode               _current;
        private CinemachineOrbitalFollow _tpOrbital;
        private CinemachineOrbitalFollow _aimOrbital;
        private CinemachinePanTilt       _fpPanTilt;
        private float                    _pendingFPYaw;
        private bool                     _isAiming;
        private bool                     _hasPendingBlend;
        private CameraBlendSettings      _pendingBlend;
        private float                    _lastInputTime;

        public CameraMode CurrentMode => _current;
        public bool       IsAiming    => _isAiming;

        public event System.Action CameraRigChanged;

        private void Awake()
        {
            _current = _defaultMode;

            if (_thirdPersonVcam != null)
                _tpOrbital  = _thirdPersonVcam.GetComponent<CinemachineOrbitalFollow>();
            if (_aimVcam != null)
                _aimOrbital = _aimVcam.GetComponent<CinemachineOrbitalFollow>();
            if (_firstPersonVcam != null)
                _fpPanTilt  = _firstPersonVcam.GetComponent<CinemachinePanTilt>();
        }

        // Called by Character.Update() each frame while possessed.
        public void HandleLook(Vector2 lookAxis, Vector2 moveAxis)
        {
            bool hasLookInput = lookAxis.sqrMagnitude > 0.01f;
            bool hasMoveInput = moveAxis.sqrMagnitude > 0.01f;
            if (hasLookInput || hasMoveInput) _lastInputTime = Time.time;

            if (_isAiming && _aimOrbital != null)
            {
                if (hasLookInput)
                {
                    _aimOrbital.HorizontalAxis.Value += lookAxis.x * _sensitivity;

                    var vAxis = _aimOrbital.VerticalAxis;
                    _aimOrbital.VerticalAxis.Value = Mathf.Clamp(
                        vAxis.Value + lookAxis.y * _sensitivity,
                        vAxis.Range.x, vAxis.Range.y);
                }
            }
            else if (_current == CameraMode.ThirdPerson && _tpOrbital != null)
            {
                if (hasLookInput)
                {
                    _tpOrbital.HorizontalAxis.Value += lookAxis.x * _sensitivity;

                    var vAxis = _tpOrbital.VerticalAxis;
                    _tpOrbital.VerticalAxis.Value = Mathf.Clamp(
                        vAxis.Value + lookAxis.y * _sensitivity,
                        vAxis.Range.x, vAxis.Range.y);
                }
                else if (!hasMoveInput && Time.time - _lastInputTime >= _returnDelay)
                {
                    _tpOrbital.HorizontalAxis.Value = Mathf.LerpAngle(
                        _tpOrbital.HorizontalAxis.Value,
                        transform.eulerAngles.y,
                        _returnSpeed * Time.deltaTime);
                }
            }
            else if (_current == CameraMode.FirstPerson && _fpPanTilt != null)
            {
                // Horizontal pan is baked into Character body rotation (see ConsumeFPBodyYawDelta).
                // PanTilt only handles vertical tilt to avoid double-rotation with the body.
                _pendingFPYaw            = lookAxis.x * _sensitivity;
                _fpPanTilt.PanAxis.Value = 0f;

                var tAxis = _fpPanTilt.TiltAxis;
                _fpPanTilt.TiltAxis.Value = Mathf.Clamp(
                    tAxis.Value - lookAxis.y * _sensitivity,
                    tAxis.Range.x, tAxis.Range.y);
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
            _isAiming = false; // cancel aim when switching camera mode
            _current  = _current == CameraMode.ThirdPerson
                ? CameraMode.FirstPerson
                : CameraMode.ThirdPerson;

            if (_current == CameraMode.FirstPerson && _fpPanTilt != null)
                _fpPanTilt.PanAxis.Value = 0f;

            CameraRigChanged?.Invoke();
        }

        public void SetAimMode(bool aim)
        {
            if (_isAiming == aim) return;

            Debug.Log($"[AimCamera] SetAimMode({aim}) | _aimVcam={_aimVcam} | _aimOrbital={_aimOrbital} | _tpOrbital={_tpOrbital}");

            // Sync orbital axes so the camera doesn't jump on aim entry/exit
            if (aim && _aimOrbital != null && _tpOrbital != null)
            {
                _aimOrbital.HorizontalAxis.Value = _tpOrbital.HorizontalAxis.Value;
                _aimOrbital.VerticalAxis.Value   = _tpOrbital.VerticalAxis.Value;
            }
            else if (!aim && _aimOrbital != null && _tpOrbital != null)
            {
                _tpOrbital.HorizontalAxis.Value = _aimOrbital.HorizontalAxis.Value;
                _tpOrbital.VerticalAxis.Value   = _aimOrbital.VerticalAxis.Value;
            }

            _isAiming        = aim;
            _hasPendingBlend = true;
            _pendingBlend    = new CameraBlendSettings(0.15f, CameraBlendStyle.EaseInOut);
            CameraRigChanged?.Invoke();
        }

        /// <summary>
        /// World-space yaw (degrees) that the active orbital is currently at.
        /// Use this for character body rotation in aim mode to avoid camera-character feedback loop.
        /// </summary>
        public float GetAimYaw()
        {
            if (_isAiming && _aimOrbital != null)
                return _aimOrbital.HorizontalAxis.Value;
            if (_tpOrbital != null)
                return _tpOrbital.HorizontalAxis.Value;
            return transform.eulerAngles.y;
        }

        public CameraRigHandle GetActiveCameraRig()
        {
            if (_isAiming && _current == CameraMode.ThirdPerson && _aimVcam != null)
                return new CameraRigHandle(_aimVcam);
            return new CameraRigHandle(_current == CameraMode.ThirdPerson ? _thirdPersonVcam : _firstPersonVcam);
        }

        public CameraBlendSettings GetBlendSettings()
        {
            if (_hasPendingBlend)
            {
                _hasPendingBlend = false;
                return _pendingBlend;
            }
            return CameraBlendSettings.Cut;
        }
    }
}
