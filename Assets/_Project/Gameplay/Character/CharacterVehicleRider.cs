using UnityEngine;
using UnityEngine.Scripting;
using Game.Core;

namespace Game.Gameplay.Character
{
    public class CharacterVehicleRider : MonoBehaviour
    {
        private const float IKFadeSpeed = 5f;

        private static readonly int HashIsRiding  = Animator.StringToHash("IsRiding");
        private static readonly int HashBikeSpeed = Animator.StringToHash("BikeSpeed");

        private Renderer[]         _renderers;
        private Animator           _animator;
        private CharacterIKPass    _ikPass;

        private VehicleRiderData   _riderData;
        private IVehicleRiderState _vehicleState;
        private bool               _inVehicle;

        private float _ikWeight;
        private float _ikWeightTarget;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _animator  = GetComponentInChildren<Animator>(true);

            if (_animator != null)
            {
                // CharacterIKPass relays OnAnimatorIK (only sent to the Animator's own GO)
                // to this component which lives on the character root.
                _ikPass = _animator.gameObject.GetComponent<CharacterIKPass>()
                       ?? _animator.gameObject.AddComponent<CharacterIKPass>();
                _ikPass.IKUpdate += OnIKUpdate;
            }
        }

        [Preserve]
        private void OnDestroy()
        {
            if (_ikPass != null)
                _ikPass.IKUpdate -= OnIKUpdate;
        }

        private void Update()
        {
            _ikWeight = Mathf.MoveTowards(_ikWeight, _ikWeightTarget, IKFadeSpeed * Time.deltaTime);

            if (!_inVehicle || _animator == null || _vehicleState == null) return;
            _animator.SetFloat(HashBikeSpeed, _vehicleState.SpeedNorm);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void OnEnterVehicle(VehicleRiderData data)
        {
            bool show = data != null && !data.HideCharacter;
            SetRenderersVisible(show);
            if (!show) { _inVehicle = false; return; }

            _riderData      = data;
            _vehicleState   = data.StateSource;
            _inVehicle      = true;
            _ikWeightTarget = 1f;
            _animator?.SetBool(HashIsRiding, true);
        }

        public void OnExitVehicle()
        {
            _riderData      = null;
            _vehicleState   = null;
            _inVehicle      = false;
            _ikWeightTarget = 0f;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _animator?.SetBool(HashIsRiding,  false);
            _animator?.SetFloat(HashBikeSpeed, 0f);
            SetRenderersVisible(true);
        }

        // ── Humanoid IK ───────────────────────────────────────────────────────
        // Called each frame by CharacterIKPass.OnAnimatorIK (base layer only).
        // Pins hands and feet to the vehicle's anchor transforms.
        // Requires: Animator Controller base layer → IK Pass = enabled.

        private void OnIKUpdate(int layer)
        {
            if (layer != 0 || _animator == null || _riderData == null) return;

            SetIKGoal(AvatarIKGoal.LeftHand,  _riderData.LeftHandTarget);
            SetIKGoal(AvatarIKGoal.RightHand, _riderData.RightHandTarget);
            SetIKGoal(AvatarIKGoal.LeftFoot,  _riderData.LeftFootTarget);
            SetIKGoal(AvatarIKGoal.RightFoot, _riderData.RightFootTarget);
        }

        private void SetIKGoal(AvatarIKGoal goal, Transform anchor)
        {
            if (anchor == null)
            {
                _animator.SetIKPositionWeight(goal, 0f);
                _animator.SetIKRotationWeight(goal, 0f);
                return;
            }
            _animator.SetIKPositionWeight(goal, _ikWeight);
            _animator.SetIKRotationWeight(goal, _ikWeight);
            _animator.SetIKPosition(goal, anchor.position);
            _animator.SetIKRotation(goal, anchor.rotation);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetRenderersVisible(bool visible)
        {
            foreach (var r in _renderers) r.enabled = visible;
        }
    }
}
