using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Base MonoBehaviour for all weapons. Handles grip attachment and activation.
    /// Subclasses override Use* methods for weapon-specific behaviour.</summary>
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        [SerializeField] protected string _weaponName = "Weapon";

        protected Transform _gripPoint;

        // IWeaponStats — override in subclasses that have ammo / aiming
        public virtual string WeaponName  => _weaponName;
        public virtual int    CurrentAmmo => -1;   // -1 = infinite (melee)
        public virtual int    ReserveAmmo => -1;
        public virtual bool   IsReloading => false;
        public virtual float  AimProgress => 0f;

        // IWeapon
        public virtual bool IsConsumed => false;   // override in Throwable/Consumable

        public virtual void Equip(Transform gripPoint)
        {
            _gripPoint = gripPoint;
            transform.SetParent(gripPoint, worldPositionStays: false);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
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
