using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Base MonoBehaviour for all weapons. Handles grip attachment and activation.
    /// Subclasses override Use* methods for weapon-specific behaviour.</summary>
    public abstract class WeaponBase : MonoBehaviour, IWeapon, IWeaponIKProvider
    {
        [SerializeField] protected string _weaponName = "Weapon";

        [Header("Animator")]
        [SerializeField] private int _weaponTypeId = 1; // 0=none, 1=1H pistol, 2=2H rifle

        [Header("Equip Offset")]
        [SerializeField] private Vector3 _equipPositionOffset;
        [SerializeField] private Vector3 _equipRotationOffset;

        [Header("IK Targets")]
        [SerializeField] private Transform _rightHandIKTarget;
        [SerializeField] private Transform _leftHandIKTarget;
        [Range(0f, 1f)]
        [SerializeField] private float _leftHandIKWeight = 0f; // 0=1H, 1=2H

        protected Transform _gripPoint;

        // IWeaponStats — override in subclasses that have ammo / aiming
        public virtual string WeaponName  => _weaponName;
        public virtual int    WeaponTypeId => _weaponTypeId;
        public virtual int    CurrentAmmo => -1;   // -1 = infinite (melee)
        public virtual int    ReserveAmmo => -1;
        public virtual bool   IsReloading => false;
        public virtual float  AimProgress => 0f;

        // IWeaponIKProvider
        public Transform RightHandIKTarget => _rightHandIKTarget;
        public Transform LeftHandIKTarget  => _leftHandIKTarget;
        public float     LeftHandIKWeight  => _leftHandIKWeight;

        // IWeapon
        public virtual bool IsConsumed => false;   // override in Throwable/Consumable

        public virtual void Equip(Transform gripPoint)
        {
            _gripPoint = gripPoint;
            transform.SetParent(gripPoint, worldPositionStays: false);
            transform.SetLocalPositionAndRotation(
                _equipPositionOffset,
                Quaternion.Euler(_equipRotationOffset));
            gameObject.SetActive(true);
        }

        public virtual void Unequip()
        {
            gameObject.SetActive(false);
            transform.SetParent(null);
            _gripPoint = null;
        }

        public abstract void UsePrimary();
        public virtual  void UseSecondary()  { }
        public virtual  void StopSecondary() { }
        public virtual  void Reload()        { }
    }
}
