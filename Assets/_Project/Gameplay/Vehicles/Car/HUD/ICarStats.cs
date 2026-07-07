namespace Game.Gameplay.Vehicles.Car
{
    public enum GearState { Reverse = -1, Neutral = 0, Drive = 1 }

    public interface ICarStats
    {
        float     SpeedKmh    { get; }
        GearState CurrentGear { get; }
    }
}
