using UnityEngine;
using UnityEngine.Scripting;
using Game.Core.Weapons;

namespace Game.Gameplay.Character
{
    public class CharacterWeaponIK : MonoBehaviour
    {
        private const float IKFadeSpeed = 8f;

        private Animator              _animator;
        private CharacterIKPass       _ikPass;
        private IWeaponHolder         _weaponHolder;
        private CharacterVehicleRider _vehicleRider;

        private float _ikWeight;
        private float _ikWeightTarget;

        private void Awake()
        {
            _weaponHolder = GetComponent<IWeaponHolder>();
            _vehicleRider = GetComponent<CharacterVehicleRider>();
            _animator     = GetComponentInChildren<Animator>(true);

            if (_animator != null)
            {
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
            bool inVehicle = _vehicleRider != null && _vehicleRider.IsInVehicle;
            bool hasIK     = !inVehicle
                          && _weaponHolder?.CurrentWeapon is IWeaponIKProvider p
                          && p.RightHandIKTarget != null;

            _ikWeightTarget = hasIK ? 1f : 0f;
            _ikWeight = Mathf.MoveTowards(_ikWeight, _ikWeightTarget, IKFadeSpeed * Time.deltaTime);
        }

        private void OnIKUpdate(int layer)
        {
            if (layer != 0 || _animator == null) return;

            var provider = _weaponHolder?.CurrentWeapon as IWeaponIKProvider;
            if (provider == null || _ikWeight <= 0f) return;

            SetIKGoal(AvatarIKGoal.RightHand, provider.RightHandIKTarget, _ikWeight);
            SetIKGoal(AvatarIKGoal.LeftHand,  provider.LeftHandIKTarget,  _ikWeight * provider.LeftHandIKWeight);
        }

        private void SetIKGoal(AvatarIKGoal goal, Transform anchor, float weight)
        {
            if (anchor == null || weight <= 0.001f)
            {
                _animator.SetIKPositionWeight(goal, 0f);
                _animator.SetIKRotationWeight(goal, 0f);
                return;
            }
            _animator.SetIKPositionWeight(goal, weight);
            _animator.SetIKRotationWeight(goal, weight);
            _animator.SetIKPosition(goal, anchor.position);
            _animator.SetIKRotation(goal, anchor.rotation);
        }
    }
}
