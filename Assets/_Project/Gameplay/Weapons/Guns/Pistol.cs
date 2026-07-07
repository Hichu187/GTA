namespace Game.Gameplay.Weapons
{
    public sealed class Pistol : GunBase
    {
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName   = "Pistol";
            _magazineSize = 12;
            _reserveAmmo  = 48;
            _fireRate     = 4f;
            _range        = 40f;
            _damage       = 25f;
            _reloadTime   = 1.2f;
        }
#endif
    }
}
