using System.Collections;
using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Base for physics-launched throwables (grenades etc.).
    /// Requires a Rigidbody on the same GO. After throw, detaches from grip and
    /// becomes a live physics object. IsConsumed = true once thrown.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ThrowableBase : WeaponBase
    {
        [Header("Throwable — Stats")]
        [SerializeField] protected float _throwForce    = 15f;
        [SerializeField] protected float _throwUpAngle  = 0.25f;  // upward bias on throw vector

        [Header("Throwable — Cook (optional)")]
        [SerializeField] protected float _maxCookTime   = 3.5f;   // auto-throw if held this long

        protected Rigidbody _rb;
        protected bool      _hasBeenThrown;
        private   float     _cookStartTime = float.MaxValue;

        public override bool IsConsumed => _hasBeenThrown;

        // Throwables have no persistent ammo display (-1 = not shown)
        public override int CurrentAmmo => -1;
        public override int ReserveAmmo => -1;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public override void Equip(Transform gripPoint)
        {
            base.Equip(gripPoint);
            _hasBeenThrown   = false;
            _cookStartTime   = float.MaxValue;
            _rb.isKinematic  = true;
            _rb.linearVelocity       = Vector3.zero;
            _rb.angularVelocity      = Vector3.zero;
        }

        // UsePrimary = immediate throw
        public override void UsePrimary()
        {
            if (_hasBeenThrown) return;
            Throw();
        }

        // UseSecondary = start cooking; StopSecondary = throw
        public override void UseSecondary()
        {
            if (_hasBeenThrown) return;
            if (_cookStartTime == float.MaxValue)
                _cookStartTime = Time.time;

            if (Time.time - _cookStartTime >= _maxCookTime)
                Throw();
        }

        public override void StopSecondary()
        {
            if (_hasBeenThrown) return;
            if (_cookStartTime < float.MaxValue)
                Throw();
        }

        protected virtual void Throw()
        {
            _hasBeenThrown = true;

            transform.SetParent(null);
            _rb.isKinematic = false;

            Vector3 dir = _gripPoint != null ? _gripPoint.forward : transform.forward;
            dir += Vector3.up * _throwUpAngle;
            _rb.AddForce(dir.normalized * _throwForce, ForceMode.Impulse);
            _rb.AddTorque(Vector3.right * _throwForce * 0.5f, ForceMode.Impulse);

            OnThrown();
        }

        /// <summary>Called immediately after throw — start fuse, trigger animation, etc.</summary>
        protected abstract void OnThrown();

        public override void Unequip()
        {
            if (_hasBeenThrown) return;  // already detached — don't hide
            base.Unequip();
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _weaponName = GetType().Name;
        }
#endif
    }
}
