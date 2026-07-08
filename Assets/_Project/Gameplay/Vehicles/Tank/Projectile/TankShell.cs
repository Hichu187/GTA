using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Vehicles.Tank
{
    [RequireComponent(typeof(Rigidbody))]
    public class TankShell : MonoBehaviour
    {
        private float _damage;
        private float _explosionRadius;
        private float _explosionForce;

        public void Init(float damage, float explosionRadius, float explosionForce, float lifetime = 6f)
        {
            _damage          = damage;
            _explosionRadius = explosionRadius;
            _explosionForce  = explosionForce;
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision _) => Explode();

        private void Explode()
        {
            var cols = Physics.OverlapSphere(transform.position, _explosionRadius);
            foreach (var col in cols)
            {
                var rb = col.attachedRigidbody;
                if (rb != null)
                    rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius,
                        1f, ForceMode.Impulse);

                col.GetComponentInParent<IDamageable>()
                   ?.TakeDamage(_damage, DamageType.Explosion);
            }

            Destroy(gameObject);
        }
    }
}
