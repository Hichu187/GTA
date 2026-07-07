using UnityEngine;
using TMPro;
using Game.Core.HUD;
using Game.Core.Weapons;

namespace Game.Gameplay.Weapons
{
    /// <summary>Displays current / reserve ammo for the active weapon.
    /// Hides itself automatically when no weapon is equipped or weapon has infinite ammo (melee).</summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class WeaponAmmoModule : MonoBehaviour, IHUDModule
    {
        private TextMeshProUGUI _text;
        private IWeaponHolder   _holder;

        private void Awake() => _text = GetComponent<TextMeshProUGUI>();

        public void Bind(object dataSource) => _holder = dataSource as IWeaponHolder;
        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            var weapon = _holder?.CurrentWeapon;
            if (weapon == null || weapon.CurrentAmmo < 0)
            {
                _text.text = string.Empty;
                return;
            }

            _text.text = weapon.IsReloading
                ? "RELOADING..."
                : $"{weapon.CurrentAmmo} / {weapon.ReserveAmmo}";
        }
    }
}
