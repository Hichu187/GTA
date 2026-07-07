using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    public sealed class MedKit : ConsumableBase
    {
        [Header("MedKit — Heal")]
        [SerializeField] private float _healAmount = 50f;

        protected override void ApplyEffect()
        {
            // Search up the hierarchy for IDamageable (the character holding this)
            var damageable = GetComponentInParent<IDamageable>()
                          ?? _gripPoint?.root.GetComponent<IDamageable>();

            // Negative damage = healing; Character.TakeDamage clamps to [0, maxHealth]
            damageable?.TakeDamage(-_healAmount, DamageType.Bullet);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName = "Med Kit";
            _useTime    = 1.5f;
            _healAmount = 50f;
        }
#endif
    }
}
