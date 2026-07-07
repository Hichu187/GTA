using System.Collections;
using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Base for single-use consumable items (medkit etc.).
    /// IsConsumed = true after use, triggering WeaponHolder slot removal.</summary>
    public abstract class ConsumableBase : WeaponBase
    {
        [Header("Consumable — Stats")]
        [SerializeField] protected float _useTime = 1.5f;  // channeled use duration

        protected bool _hasBeenUsed;
        protected bool _isUsing;

        public override bool IsConsumed => _hasBeenUsed;

        // Consumables have no ammo display
        public override int CurrentAmmo => -1;
        public override int ReserveAmmo => -1;

        public override void Equip(Transform gripPoint)
        {
            base.Equip(gripPoint);
            _hasBeenUsed = false;
            _isUsing     = false;
        }

        public override void UsePrimary()
        {
            if (_hasBeenUsed || _isUsing) return;
            StartCoroutine(UseRoutine());
        }

        private IEnumerator UseRoutine()
        {
            _isUsing = true;
            yield return new WaitForSeconds(_useTime);
            if (!_hasBeenUsed)  // might have been interrupted in future
            {
                ApplyEffect();
                _hasBeenUsed = true;
            }
            _isUsing = false;
        }

        /// <summary>Apply the item's effect to the user or world.</summary>
        protected abstract void ApplyEffect();

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _weaponName = GetType().Name;
        }
#endif
    }
}
