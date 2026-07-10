using System.Collections;
using UnityEngine;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Base for all hitscan firearms. Subclasses override Fire() for pellet spread etc.</summary>
    public abstract class GunBase : WeaponBase
    {
        [Header("Gun — Stats")]
        [SerializeField] protected int       _magazineSize = 12;
        [SerializeField] protected int       _reserveAmmo  = 48;
        [SerializeField] protected float     _fireRate     = 4f;     // shots per second
        [SerializeField] protected float     _range        = 50f;
        [SerializeField] protected float     _damage       = 25f;
        [SerializeField] protected float     _reloadTime   = 1.5f;

        [Header("Gun — Visual")]
        [SerializeField] protected Transform   _muzzlePoint;
        [SerializeField] protected GameObject  _muzzleFlashPrefab;
        [SerializeField] protected float       _muzzleFlashDuration = 0.05f;

        [Header("Gun — Hit")]
        [SerializeField] protected LayerMask _hitLayers = ~0;

        protected int   _currentAmmo;
        protected bool  _isReloading;
        protected bool  _isAiming;
        protected float _aimProgress;
        private   float _nextFireTime;

        // IWeaponStats
        public override int   CurrentAmmo => _currentAmmo;
        public override int   ReserveAmmo => _reserveAmmo;
        public override bool  IsReloading => _isReloading;
        public override float AimProgress => _aimProgress;

        protected virtual void Awake()
        {
            _currentAmmo = _magazineSize;
        }

        public override void Equip(Transform gripPoint)
        {
            base.Equip(gripPoint);
            _isReloading = false;
        }

        // ── Primary: fire ─────────────────────────────────────────────────────
        public override void UsePrimary()
        {
            if (_isReloading || Time.time < _nextFireTime) return;

            if (_currentAmmo <= 0)
            {
                Reload();
                return;
            }

            _currentAmmo--;
            _nextFireTime = Time.time + 1f / _fireRate;
            SpawnMuzzleFlash();
            Fire();
        }

        /// <summary>Override to customise projectile pattern (e.g. shotgun pellet spread).</summary>
        protected virtual void Fire()
        {
            FireRaycast(GetFireDirection(), _damage);
        }

        protected Vector3 GetFireDirection()
        {
            if (_isAiming)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    // Raycast from screen centre — bullet goes where crosshair points
                    var ray    = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    var origin = _muzzlePoint != null ? _muzzlePoint.position : transform.position;
                    return Physics.Raycast(ray, out RaycastHit hit, _range, _hitLayers)
                        ? (hit.point - origin).normalized
                        : ray.direction;
                }
            }
            return _gripPoint != null ? _gripPoint.forward : transform.forward;
        }

        protected void FireRaycast(Vector3 direction, float damage)
        {
            Transform origin = _muzzlePoint != null ? _muzzlePoint : transform;
            if (Physics.Raycast(origin.position, direction, out RaycastHit hit, _range, _hitLayers))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var d))
                    d.TakeDamage(damage, DamageType.Bullet);
            }
        }

        // ── Secondary: aim ────────────────────────────────────────────────────
        public override void UseSecondary()
        {
            _isAiming = true;
        }

        public override void StopSecondary()
        {
            _isAiming = false;
        }

        protected virtual void Update()
        {
            // Smooth aim progress for camera blend
            float target = _isAiming ? 1f : 0f;
            _aimProgress = Mathf.MoveTowards(_aimProgress, target, Time.deltaTime * 8f);
        }

        // ── Reload ────────────────────────────────────────────────────────────
        public override void Reload()
        {
            if (_isReloading || _currentAmmo >= _magazineSize || _reserveAmmo <= 0) return;
            StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            _isReloading = true;
            yield return new WaitForSeconds(_reloadTime);

            int needed   = _magazineSize - _currentAmmo;
            int take     = Mathf.Min(needed, _reserveAmmo);
            _currentAmmo += take;
            _reserveAmmo -= take;
            _isReloading  = false;
        }

        // ── Ammo setter (used by save/load restore) ───────────────────────────
        public void SetAmmo(int magazine, int reserve)
        {
            _currentAmmo = Mathf.Clamp(magazine, 0, _magazineSize);
            _reserveAmmo = Mathf.Max(reserve, 0);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void SpawnMuzzleFlash()
        {
            if (_muzzleFlashPrefab == null || _muzzlePoint == null) return;
            var flash = Instantiate(_muzzleFlashPrefab, _muzzlePoint.position, _muzzlePoint.rotation);
            Destroy(flash, _muzzleFlashDuration);
        }

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            _weaponName = GetType().Name;
        }
#endif
    }
}
