namespace Game.Gameplay.Weapons
{
    public sealed class Knife : MeleeBase
    {
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName     = "Knife";
            _damage         = 35f;
            _heavyDamage    = 70f;
            _attackRange    = 1.5f;
            _attackCooldown = 0.35f;
            _heavyCooldown  = 0.9f;
        }
#endif
    }
}
