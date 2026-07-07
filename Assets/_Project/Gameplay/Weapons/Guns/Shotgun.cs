using UnityEngine;

namespace Game.Gameplay.Weapons
{
    public sealed class Shotgun : GunBase
    {
        [Header("Shotgun — Pellets")]
        [SerializeField] private int   _pelletCount = 8;
        [SerializeField] private float _spread      = 0.08f;   // radians per pellet

        protected override void Fire()
        {
            if (_gripPoint == null) return;

            for (int i = 0; i < _pelletCount; i++)
            {
                Vector3 dir = _gripPoint.forward
                    + new Vector3(
                        Random.Range(-_spread, _spread),
                        Random.Range(-_spread, _spread),
                        0f);
                FireRaycast(dir.normalized, _damage / _pelletCount);
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName   = "Shotgun";
            _magazineSize = 8;
            _reserveAmmo  = 24;
            _fireRate     = 1.2f;
            _range        = 25f;
            _damage       = 120f;   // split across pellets
            _reloadTime   = 2.5f;
        }
#endif
    }
}
