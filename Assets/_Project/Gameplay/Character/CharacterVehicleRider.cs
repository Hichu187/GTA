using UnityEngine;
using Game.Core;

namespace Game.Gameplay.Character
{
    // Manages character state while seated in a vehicle.
    // Phase 1 — smooth IK: proxy foot targets, global IK fade-in
    // Phase 2 — stand behaviour: one foot on ground when stopped, body counter-lean
    // Phase 3 — per-limb weights, world-space ground rotation, animator IsRiding/BikeSpeed
    public class CharacterVehicleRider : MonoBehaviour
    {
        // Hands snap quickly so they grab the handlebar immediately on enter
        private const float HandIKFadeSpeed  = 8f;
        // Feet settle slowly so they ease onto the pedals
        private const float FootIKFadeSpeed  = 3f;
        // Proxy lerp speed toward the active target (pedal or ground stand)
        private const float LegLerpSpeed     = 8f;
        // Body lean/unlean interpolation speed
        private const float BodyLeanSpeed    = 3f;
        // Lateral body shift when stopped (metres, local X)
        private const float StandBodyOffset  = 0.12f;
        // Body Z rotation to counter-lean when stopped (degrees)
        private const float StandBodyAngle   = 12f;
        // How much the torso bends toward the handlebar (0 = none, 1 = full)
        private const float SpineBodyWeight  = 0.60f;
        // How much the head follows the spine lean
        private const float SpineHeadWeight  = 0.30f;

        // Animator parameter hashes — add these params to the Animator Controller
        private static readonly int HashIsRiding  = Animator.StringToHash("IsRiding");
        private static readonly int HashBikeSpeed = Animator.StringToHash("BikeSpeed");

        private Renderer[]         _renderers;
        private Animator           _animator;
        private CharacterIKPass    _ikPass;
        private IVehicleRiderState _vehicleState;

        // Pedal anchors (while riding)
        private Transform _leftFootPedal;
        private Transform _rightFootPedal;
        // Ground-stand anchors (while stopped and leaning)
        private Transform _leftStandTarget;
        private Transform _rightStandTarget;
        // Handlebar anchors
        private Transform _leftHandTarget;
        private Transform _rightHandTarget;

        // Runtime proxy GameObjects parented to the vehicle
        private GameObject _leftLegProxy;
        private GameObject _rightLegProxy;

        // Per-limb IK weights — hands and feet fade at different speeds
        private float _handIKWeight;
        private float _footIKWeight;
        private float _ikWeightTarget;   // shared target (0 or 1) for both groups

        // IK hints — guide knee/elbow bend direction
        private Transform _leftKneeHint;
        private Transform _rightKneeHint;
        private Transform _leftElbowHint;
        private Transform _rightElbowHint;

        // Spine look target — if null, auto-computed from hand grip midpoint
        private Transform _spineLookTarget;
        // Hip/seat anchor — forces bodyPosition so shoulders are within reach of handlebar
        private Transform _seatAnchor;

        // Track which foot is on the ground so rotation source can switch
        private bool _leftFootOnGround;
        private bool _rightFootOnGround;

        private bool _inVehicle;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _animator  = GetComponentInChildren<Animator>(true);

            if (_animator != null)
            {
                _ikPass = _animator.GetComponent<CharacterIKPass>();
                if (_ikPass == null)
                    _ikPass = _animator.gameObject.AddComponent<CharacterIKPass>();
                _ikPass.IKUpdate += ApplyIK;
            }
        }

        private void OnDestroy()
        {
            if (_ikPass != null) _ikPass.IKUpdate -= ApplyIK;
        }

        private void Update()
        {
            if (!_inVehicle) return;

            // Hands reach full weight quickly, feet take longer to settle
            _handIKWeight = Mathf.MoveTowards(_handIKWeight, _ikWeightTarget, HandIKFadeSpeed * Time.deltaTime);
            _footIKWeight = Mathf.MoveTowards(_footIKWeight, _ikWeightTarget, FootIKFadeSpeed * Time.deltaTime);

            UpdateLegsAndBody();

            // Drive animator parameters from live vehicle state
            if (_animator != null && _vehicleState != null)
                _animator.SetFloat(HashBikeSpeed, _vehicleState.SpeedNorm);
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void OnEnterVehicle(VehicleRiderData data)
        {
            bool show = data != null && !data.HideCharacter;
            SetRenderersVisible(show);

            if (!show) { _inVehicle = false; return; }

            _vehicleState     = data.StateSource;
            _leftFootPedal    = data.LeftFootTarget;
            _rightFootPedal   = data.RightFootTarget;
            _leftStandTarget  = data.LeftStandTarget;
            _rightStandTarget = data.RightStandTarget;
            _leftHandTarget   = data.LeftHandTarget;
            _rightHandTarget  = data.RightHandTarget;
            _leftKneeHint     = data.LeftKneeHint;
            _rightKneeHint    = data.RightKneeHint;
            _leftElbowHint    = data.LeftElbowHint;
            _rightElbowHint   = data.RightElbowHint;
            _spineLookTarget  = data.SpineLookTarget;
            _seatAnchor       = data.SeatAnchor;

            CreateProxy(ref _leftLegProxy,  _leftFootPedal,  "LegProxy_L");
            CreateProxy(ref _rightLegProxy, _rightFootPedal, "LegProxy_R");

            _handIKWeight   = 0f;
            _footIKWeight   = 0f;
            _ikWeightTarget = 1f;
            _inVehicle      = true;

            _animator?.SetBool(HashIsRiding, true);
        }

        public void OnExitVehicle()
        {
            DestroyProxy(ref _leftLegProxy);
            DestroyProxy(ref _rightLegProxy);

            _vehicleState     = null;
            _leftFootPedal    = _rightFootPedal    = null;
            _leftStandTarget  = _rightStandTarget  = null;
            _leftHandTarget   = _rightHandTarget   = null;
            _leftKneeHint     = _rightKneeHint     = null;
            _leftElbowHint    = _rightElbowHint    = null;
            _spineLookTarget  = null;
            _seatAnchor       = null;

            _handIKWeight     = 0f;
            _footIKWeight     = 0f;
            _ikWeightTarget   = 0f;
            _leftFootOnGround = _rightFootOnGround = false;
            _inVehicle        = false;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _animator?.SetBool(HashIsRiding,  false);
            _animator?.SetFloat(HashBikeSpeed, 0f);

            SetRenderersVisible(true);
        }

        // ── Stand behaviour ──────────────────────────────────────────────────

        private void UpdateLegsAndBody()
        {
            bool hasState  = _vehicleState != null;
            bool isMoving  = !hasState || _vehicleState.IsMoving;
            bool tiltRight = hasState && _vehicleState.TiltToRight;

            if (isMoving)
            {
                _leftFootOnGround  = false;
                _rightFootOnGround = false;

                LerpProxy(_leftLegProxy,  _leftFootPedal);
                LerpProxy(_rightLegProxy, _rightFootPedal);

                transform.localPosition = Vector3.Lerp(
                    transform.localPosition, Vector3.zero, BodyLeanSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Lerp(
                    transform.localRotation, Quaternion.identity, BodyLeanSpeed * Time.deltaTime);
            }
            else
            {
                // TiltToRight → right foot on ground, left on pedal
                _leftFootOnGround  = !tiltRight;
                _rightFootOnGround =  tiltRight;

                LerpProxy(_leftLegProxy,
                    tiltRight ? _leftFootPedal  : (_leftStandTarget  ?? _leftFootPedal));
                LerpProxy(_rightLegProxy,
                    tiltRight ? (_rightStandTarget ?? _rightFootPedal) : _rightFootPedal);

                float targetX = tiltRight ?  StandBodyOffset : -StandBodyOffset;
                float targetZ = tiltRight ?  StandBodyAngle  : -StandBodyAngle;

                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    new Vector3(targetX, 0f, 0f),
                    BodyLeanSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Lerp(
                    transform.localRotation,
                    Quaternion.Euler(0f, 0f, targetZ),
                    BodyLeanSpeed * Time.deltaTime);
            }
        }

        // ── IK ───────────────────────────────────────────────────────────────

        private void ApplyIK(int layer)
        {
            if (_animator == null || !_inVehicle) return;

            // Force hips to seat position so shoulders are within arm's reach of handlebar
            if (_seatAnchor != null && _handIKWeight > 0f)
            {
                _animator.bodyPosition = Vector3.Lerp(
                    _animator.bodyPosition, _seatAnchor.position, _handIKWeight);
                _animator.bodyRotation = Quaternion.Slerp(
                    _animator.bodyRotation, _seatAnchor.rotation, _handIKWeight);
            }

            // Foot rotation: use stand target's rotation when on ground (flat-foot),
            // pedal rotation otherwise
            Transform leftRotSrc  = _leftFootOnGround  ? _leftStandTarget  : _leftFootPedal;
            Transform rightRotSrc = _rightFootOnGround ? _rightStandTarget : _rightFootPedal;

            ApplyIKGoal(AvatarIKGoal.LeftFoot,
                _leftLegProxy  != null ? _leftLegProxy.transform  : _leftFootPedal,
                leftRotSrc, _footIKWeight);

            ApplyIKGoal(AvatarIKGoal.RightFoot,
                _rightLegProxy != null ? _rightLegProxy.transform : _rightFootPedal,
                rightRotSrc, _footIKWeight);

            // Hands snap to handlebar with faster weight
            ApplyIKGoal(AvatarIKGoal.LeftHand,  _leftHandTarget,  _leftHandTarget,  _handIKWeight);
            ApplyIKGoal(AvatarIKGoal.RightHand, _rightHandTarget, _rightHandTarget, _handIKWeight);

            // Hints guide the solver on which direction to bend knees/elbows
            ApplyIKHint(AvatarIKHint.LeftKnee,   _leftKneeHint,   _footIKWeight);
            ApplyIKHint(AvatarIKHint.RightKnee,  _rightKneeHint,  _footIKWeight);
            ApplyIKHint(AvatarIKHint.LeftElbow,  _leftElbowHint,  _handIKWeight);
            ApplyIKHint(AvatarIKHint.RightElbow, _rightElbowHint, _handIKWeight);

            // Bend spine toward handlebar — always applies while riding
            if (_handIKWeight > 0f)
            {
                Vector3 lookPos;
                if (_spineLookTarget != null)
                    lookPos = _spineLookTarget.position;
                else if (_leftHandTarget != null && _rightHandTarget != null)
                    lookPos = (_leftHandTarget.position + _rightHandTarget.position) * 0.5f;
                else if (_leftHandTarget != null)
                    lookPos = _leftHandTarget.position;
                else if (_rightHandTarget != null)
                    lookPos = _rightHandTarget.position;
                else
                {
                    // No targets set — lean forward from chest
                    Transform chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
                    Vector3 origin  = chest != null ? chest.position : transform.position + Vector3.up * 1.2f;
                    lookPos = origin + transform.forward * 0.8f;
                }
                _animator.SetLookAtWeight(_handIKWeight, SpineBodyWeight, SpineHeadWeight, 0f, 0.5f);
                _animator.SetLookAtPosition(lookPos);
            }
            else
            {
                _animator.SetLookAtWeight(0f);
            }
        }

        private void ApplyIKGoal(AvatarIKGoal goal, Transform posSource, Transform rotSource, float weight)
        {
            if (posSource == null || weight <= 0f)
            {
                _animator.SetIKPositionWeight(goal, 0f);
                _animator.SetIKRotationWeight(goal, 0f);
                return;
            }
            _animator.SetIKPositionWeight(goal, weight);
            _animator.SetIKRotationWeight(goal, weight);
            _animator.SetIKPosition(goal, posSource.position);
            _animator.SetIKRotation(goal, rotSource != null ? rotSource.rotation : posSource.rotation);
        }

        private void ApplyIKHint(AvatarIKHint hint, Transform source, float weight)
        {
            if (source == null || weight <= 0f)
            {
                _animator.SetIKHintPositionWeight(hint, 0f);
                return;
            }
            _animator.SetIKHintPositionWeight(hint, weight);
            _animator.SetIKHintPosition(hint, source.position);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void CreateProxy(ref GameObject proxy, Transform anchor, string name)
        {
            if (proxy != null) Destroy(proxy);
            if (anchor == null) return;
            proxy = new GameObject(name);
            proxy.transform.SetParent(anchor.parent, worldPositionStays: true);
            proxy.transform.position = anchor.position;
        }

        private void DestroyProxy(ref GameObject proxy)
        {
            if (proxy != null) { Destroy(proxy); proxy = null; }
        }

        private void LerpProxy(GameObject proxy, Transform target)
        {
            if (proxy == null || target == null) return;
            proxy.transform.position = Vector3.Lerp(
                proxy.transform.position, target.position, LegLerpSpeed * Time.deltaTime);
        }

        private void SetRenderersVisible(bool visible)
        {
            foreach (var r in _renderers) r.enabled = visible;
        }
    }
}
