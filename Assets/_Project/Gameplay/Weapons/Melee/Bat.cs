namespace Game.Gameplay.Weapons
{
    public sealed class Bat : MeleeBase
    {
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _weaponName     = "Baseball Bat";
            _damage         = 50f;
            _heavyDamage    = 100f;
            _attackRange    = 2.0f;
            _attackCooldown = 0.7f;
            _heavyCooldown  = 1.5f;
        }
#endif
    }
}
