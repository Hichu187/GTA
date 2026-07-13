using UnityEngine;
using Game.Gameplay.Character.Locomotion;

namespace Game.Gameplay.Character.Animation
{
    // Add this component to the character root. The Animator can be on the root or any child.
    public class CharacterAnimationDriver : MonoBehaviour
    {
        [SerializeField] private float _speedDampTime = 0.10f;
        [SerializeField] private float _inputDampTime = 0.08f;

        // Parameter hashes — cached once to avoid per-frame string lookups
        private static readonly int _hashSpeed       = Animator.StringToHash("Speed");
        private static readonly int _hashMoveX       = Animator.StringToHash("MoveX");
        private static readonly int _hashMoveY       = Animator.StringToHash("MoveY");
        private static readonly int _hashIsGrounded  = Animator.StringToHash("IsGrounded");
        private static readonly int _hashIsCrouching = Animator.StringToHash("IsCrouching");
        private static readonly int _hashIsSwimming  = Animator.StringToHash("IsSwimming");
        private static readonly int _hashIsDiving    = Animator.StringToHash("IsDiving");
        private static readonly int _hashIsDrowned   = Animator.StringToHash("IsDrowned");
        private static readonly int _hashMoveZ       = Animator.StringToHash("MoveZ");
        private static readonly int _hashIsClimbing  = Animator.StringToHash("IsClimbing");
        private static readonly int _hashClimbSpeed  = Animator.StringToHash("ClimbSpeed");
        private static readonly int _hashJump        = Animator.StringToHash("Jump");
        private static readonly int _hashIsArmed     = Animator.StringToHash("IsArmed");
        private static readonly int _hashIsAiming    = Animator.StringToHash("IsAiming");
        private static readonly int _hashWeaponType  = Animator.StringToHash("WeaponType");

        private Animator                _animator;
        private ICharacterAnimationData _source;
        private LocomotionStateId       _prevState;

        private void Awake()
        {
            _animator  = GetComponentInChildren<Animator>(true);
            _source    = GetComponent<ICharacterAnimationData>();
            _prevState = LocomotionStateId.Idle;

            if (_animator == null)
                Debug.LogWarning("[CharacterAnimationDriver] No Animator found on " + gameObject.name + " or its children.", this);
            if (_source == null)
                Debug.LogWarning("[CharacterAnimationDriver] No ICharacterAnimationData found on " + gameObject.name, this);
        }

        private void Update()
        {
            if (_source == null || _animator == null) return;

            if (_source.IsDrowned)
            {
                // Underwater death — frozen in the SwimDrowned pose (Character.Die() skips
                // ragdoll for this case). IsAnimationActive is false at this point too, so
                // this check must come first or the branch below would force Idle instead.
                _animator.SetBool(_hashIsDrowned, true);
                return;
            }

            if (!_source.IsAnimationActive)
            {
                // Character is in a vehicle or unpossessed — freeze at idle/grounded
                _animator.SetFloat(_hashSpeed,      0f);
                _animator.SetFloat(_hashMoveX,      0f);
                _animator.SetFloat(_hashMoveY,      0f);
                _animator.SetFloat(_hashMoveZ,      0f);
                _animator.SetBool(_hashIsGrounded,  true);
                _animator.SetBool(_hashIsCrouching, false);
                _animator.SetBool(_hashIsSwimming,  false);
                _animator.SetBool(_hashIsDiving,    false);
                _animator.SetBool(_hashIsClimbing,  false);
                _animator.SetFloat(_hashClimbSpeed, 0f);
                _animator.SetBool(_hashIsArmed,     false);
                _animator.SetBool(_hashIsAiming,    false);
                _animator.SetInteger(_hashWeaponType, 0);
                _prevState = LocomotionStateId.Idle;
                return;
            }

            float dt = Time.deltaTime;

            float speedNorm = _source.MaxMoveSpeed > 0f
                ? _source.MoveSpeed / _source.MaxMoveSpeed
                : 0f;

            var scaledInput = _source.MoveInput * speedNorm;
            _animator.SetFloat(_hashSpeed,  speedNorm,        _speedDampTime, dt);
            _animator.SetFloat(_hashMoveX,  scaledInput.x,    _inputDampTime, dt);
            _animator.SetFloat(_hashMoveY,  scaledInput.y,    _inputDampTime, dt);
            _animator.SetBool(_hashIsGrounded,  _source.IsGrounded);
            _animator.SetBool(_hashIsCrouching, _source.IsCrouching);
            _animator.SetBool(_hashIsArmed,     _source.IsArmed);
            _animator.SetBool(_hashIsAiming,    _source.IsAiming);
            _animator.SetInteger(_hashWeaponType, _source.WeaponType);

            var state = _source.LocomotionState;
            _animator.SetBool(_hashIsSwimming, state == LocomotionStateId.Swim);
            _animator.SetBool(_hashIsDiving,   state == LocomotionStateId.Dive);
            // Optional third blend axis for a 3D swim/dive blend tree — 0 on land,
            // -1..1 while in water (negative = diving down, positive = swimming up).
            _animator.SetFloat(_hashMoveZ, _source.SwimVerticalInput, _inputDampTime, dt);

            _animator.SetBool(_hashIsClimbing, _source.IsClimbing);
            // Optional — lets a Climb animation state reverse playback speed when
            // climbing down (negative) vs up (positive) via a Speed multiplier parameter.
            _animator.SetFloat(_hashClimbSpeed, _source.ClimbVerticalInput, _inputDampTime, dt);

            // Jump trigger fires once on FSM state entry
            if (state != _prevState)
            {
                if (state == LocomotionStateId.Jump)
                    _animator.SetTrigger(_hashJump);
                _prevState = state;
            }
        }
    }
}
