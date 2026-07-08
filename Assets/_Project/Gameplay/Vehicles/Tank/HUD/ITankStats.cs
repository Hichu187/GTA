namespace Game.Gameplay.Vehicles.Tank
{
    public interface ITankStats
    {
        float SpeedKmh         { get; }
        int   AmmoCount        { get; }
        // 0 = cannon ready, 1 = just fired (reloading)
        float FireCooldownRatio { get; }
    }
}
