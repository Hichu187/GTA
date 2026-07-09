namespace Game.Core
{
    // Implement on any vehicle that controls character visibility / IK while occupied.
    // Character reads this after being seated via GetComponentInParent.
    // Vehicles that don't implement it default to hiding the character.
    public interface IVehicleRiderSource
    {
        VehicleRiderData GetRiderData();
    }
}
