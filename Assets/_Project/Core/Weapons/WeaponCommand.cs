namespace Game.Core.Weapons
{
    public readonly struct WeaponCommand
    {
        public readonly bool  FirePressed;
        public readonly bool  AimHeld;
        public readonly bool  ReloadPressed;
        public readonly float SwitchDelta;    // +1 next / -1 prev (scroll or D-pad)
        public readonly bool  ThrowPressed;

        public WeaponCommand(bool firePressed, bool aimHeld, bool reloadPressed,
                             float switchDelta, bool throwPressed)
        {
            FirePressed   = firePressed;
            AimHeld       = aimHeld;
            ReloadPressed = reloadPressed;
            SwitchDelta   = switchDelta;
            ThrowPressed  = throwPressed;
        }

        public static readonly WeaponCommand Empty = default;
    }
}
