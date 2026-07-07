using UnityEngine;

namespace Game.Core.Weapons
{
    public interface IWeapon : IWeaponStats
    {
        /// <summary>True when a single-use item is expended (thrown grenade, used medkit).
        /// WeaponHolder removes the slot automatically when this becomes true.</summary>
        bool IsConsumed { get; }

        void Equip(Transform gripPoint);
        void Unequip();

        void UsePrimary();       // fire / swing / throw
        void UseSecondary();     // aim / heavy attack / cook
        void StopSecondary();
        void Reload();
    }
}
