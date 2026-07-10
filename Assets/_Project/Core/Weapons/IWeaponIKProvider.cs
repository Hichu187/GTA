using UnityEngine;

namespace Game.Core.Weapons
{
    public interface IWeaponIKProvider
    {
        Transform RightHandIKTarget { get; }
        Transform LeftHandIKTarget  { get; }
        float     LeftHandIKWeight  { get; } // 0 = 1H pistol, 1 = 2H rifle
    }
}
