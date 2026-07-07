namespace Game.Gameplay.Vehicles.Helicopter
{
    public interface IHelicopterStats
    {
        float SpeedKmh        { get; }  // horizontal speed
        float AltitudeM       { get; }  // world Y
        float VerticalSpeedMs { get; }  // positive = ascending, negative = descending
    }
}
