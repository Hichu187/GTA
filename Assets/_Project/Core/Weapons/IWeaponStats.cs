namespace Game.Core.Weapons
{
    public interface IWeaponStats
    {
        string WeaponName    { get; }
        int    CurrentAmmo   { get; }
        int    ReserveAmmo   { get; }
        bool   IsReloading   { get; }
        float  AimProgress   { get; }   // 0 = hip, 1 = fully aimed (for camera blend)
    }
}
