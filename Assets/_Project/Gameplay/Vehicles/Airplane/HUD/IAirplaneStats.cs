namespace Game.Gameplay.Vehicles.Airplane
{
    public interface IAirplaneStats
    {
        float SpeedKmh   { get; }  // forward speed
        float AltitudeM  { get; }  // world Y position
        float HeadingDeg { get; }  // yaw 0–359, N=0, E=90
        float ThrottlePct { get; } // 0–100
    }
}
