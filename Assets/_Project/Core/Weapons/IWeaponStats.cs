namespace Game.Core.Weapons
{
    public interface IWeaponStats
    {
        string WeaponName    { get; }
        int    WeaponTypeId  { get; }   // 0=none, 1=1H pistol, 2=2H rifle/shotgun
        int    CurrentAmmo   { get; }
        int    ReserveAmmo   { get; }
        bool   IsReloading   { get; }
        float  AimProgress   { get; }   // 0 = hip, 1 = fully aimed (for camera blend)
    }
}
