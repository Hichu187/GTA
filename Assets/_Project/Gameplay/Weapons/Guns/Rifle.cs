namespace Game.Gameplay.Weapons
{
    public sealed class Rifle : GunBase
    {
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName   = "Assault Rifle";
            _magazineSize = 30;
            _reserveAmmo  = 90;
            _fireRate     = 10f;
            _range        = 80f;
            _damage       = 20f;
            _reloadTime   = 2.0f;
        }
#endif
    }
}
