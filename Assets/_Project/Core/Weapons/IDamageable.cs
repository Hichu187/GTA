namespace Game.Core.Weapons
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        void TakeDamage(float amount, DamageType type);
    }
}
