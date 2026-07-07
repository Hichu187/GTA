using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Base for all melee weapons. Uses a SphereCast forward for hit detection.</summary>
    public abstract class MeleeBase : WeaponBase
    {
        [Header("Melee — Stats")]
        [SerializeField] protected float     _damage         = 30f;
        [SerializeField] protected float     _heavyDamage    = 60f;   // UseSecondary
        [SerializeField] protected float     _attackRange    = 1.8f;
        [SerializeField] protected float     _hitRadius      = 0.25f;
        [SerializeField] protected float     _attackCooldown = 0.45f;
        [SerializeField] protected float     _heavyCooldown  = 1.0f;
        [SerializeField] protected LayerMask _hitLayers      = ~0;

        private float _nextAttackTime;

        // Melee has no ammo
        public override int   CurrentAmmo => -1;
        public override int   ReserveAmmo => -1;

        // ── Primary: light attack ─────────────────────────────────────────────
        public override void UsePrimary()
        {
            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + _attackCooldown;
            Attack(_damage);
        }

        // ── Secondary: heavy attack ───────────────────────────────────────────
        public override void UseSecondary()
        {
            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + _heavyCooldown;
            Attack(_heavyDamage);
        }

        protected virtual void Attack(float damage)
        {
            if (_gripPoint == null) return;

            Transform origin = _muzzlePoint != null ? _muzzlePoint : _gripPoint;
            var hits = Physics.SphereCastAll(
                origin.position, _hitRadius, _gripPoint.forward, _attackRange, _hitLayers);

            foreach (var hit in hits)
            {
                // Skip own hierarchy (character holding the weapon)
                if (hit.transform.IsChildOf(_gripPoint.root)) continue;

                if (hit.collider.TryGetComponent<IDamageable>(out var d))
                    d.TakeDamage(damage, DamageType.Melee);
            }
        }

        // Melee weapons have a visual muzzle point (tip of weapon) — optional
        [SerializeField] protected Transform _muzzlePoint;

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _weaponName = GetType().Name;
        }
#endif
    }
}
