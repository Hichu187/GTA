namespace Game.Gameplay.Vehicles.Rocket
{
    public interface IRocketStats
    {
        float SpeedKmh    { get; }
        float AltitudeM   { get; }
        float ThrottlePct { get; }
    }
}
