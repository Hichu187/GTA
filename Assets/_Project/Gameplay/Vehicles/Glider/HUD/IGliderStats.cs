namespace Game.Gameplay.Vehicles.Glider
{
    public interface IGliderStats
    {
        float SpeedKmh        { get; }  // forward speed
        float AltitudeM       { get; }  // world Y
        float VerticalSpeedMs { get; }  // sink rate: negative = descending
    }
}
