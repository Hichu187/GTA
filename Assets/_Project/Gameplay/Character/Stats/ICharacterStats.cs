namespace Game.Gameplay.Character.Stats
{
    public interface ICharacterStats
    {
        float Health    { get; }
        float MaxHealth { get; }
        float Stamina   { get; }
        float MaxStamina{ get; }
    }
}
