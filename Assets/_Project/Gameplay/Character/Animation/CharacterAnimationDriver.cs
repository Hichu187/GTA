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
        private static readonly int _hashJump        = Animator.StringToHash("Jump");

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

            if (!_source.IsAnimationActive)
            {
                // Character is in a vehicle or unpossessed — freeze at idle/grounded
                _animator.SetFloat(_hashSpeed,      0f);
                _animator.SetFloat(_hashMoveX,      0f);
                _animator.SetFloat(_hashMoveY,      0f);
                _animator.SetBool(_hashIsGrounded,  true);
                _animator.SetBool(_hashIsCrouching, false);
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

            // Jump trigger fires once on FSM state entry
            var state = _source.LocomotionState;
            if (state != _prevState)
            {
                if (state == LocomotionStateId.Jump)
                    _animator.SetTrigger(_hashJump);
                _prevState = state;
            }
        }
    }
}
