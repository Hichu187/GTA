using System.Collections;
using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    public sealed class Grenade : ThrowableBase
    {
        [Header("Grenade — Explosion")]
        [SerializeField] private float     _fuseTime    = 3f;
        [SerializeField] private float     _blastRadius = 5f;
        [SerializeField] private float     _damage      = 150f;
        [SerializeField] private LayerMask _hitLayers   = ~0;

        [Header("Grenade — VFX (optional)")]
        [SerializeField] private GameObject _explosionVfxPrefab;

        protected override void OnThrown()
        {
            StartCoroutine(FuseRoutine());
        }

        private IEnumerator FuseRoutine()
        {
            yield return new WaitForSeconds(_fuseTime);
            Explode();
        }

        private void Explode()
        {
            if (_explosionVfxPrefab != null)
                Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);

            var cols = Physics.OverlapSphere(transform.position, _blastRadius, _hitLayers);
            foreach (var col in cols)
            {
                if (col.TryGetComponent<IDamageable>(out var d))
                {
                    float dist    = Vector3.Distance(transform.position, col.transform.position);
                    float falloff = 1f - Mathf.Clamp01(dist / _blastRadius);
                    d.TakeDamage(_damage * falloff, DamageType.Explosion);
                }
            }

            Destroy(gameObject);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName  = "Grenade";
            _throwForce  = 15f;
            _maxCookTime = 3.5f;
        }
#endif
    }
}
