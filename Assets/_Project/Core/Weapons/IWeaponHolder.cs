namespace Game.Core.Weapons
{
    public interface IWeaponHolder
    {
        IWeapon CurrentWeapon { get; }
        int     SlotCount     { get; }

        void Tick(WeaponCommand cmd);
        bool PickUp(IWeapon weapon);   // returns false if inventory full
        void Drop();
        void SwitchTo(int slotIndex);
    }
}
