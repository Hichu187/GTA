using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Weapons;
using Game.Gameplay.Character.Stats;

namespace Game.Gameplay.Character
{
    public class CharacterHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private GameObject _staminaBarPrefab;
        [SerializeField] private GameObject _crosshairPrefab;

        [Header("Weapon HUD (auto-used when WeaponHolder is present)")]
        [SerializeField] private GameObject _weaponAmmoPrefab;
        [SerializeField] private GameObject _weaponNamePrefab;

        public ICharacterStats StatsSource { get; set; }

        private IWeaponHolder _weaponHolder;

        private void Awake()
        {
            _weaponHolder = GetComponent<IWeaponHolder>();
        }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("HealthBar",  _healthBarPrefab,  StatsSource),
                new HUDModuleHandle("StaminaBar", _staminaBarPrefab, StatsSource),
                new HUDModuleHandle("Crosshair",  _crosshairPrefab,  null),
            };

            if (_weaponHolder != null)
            {
                if (_weaponAmmoPrefab != null)
                    modules.Add(new HUDModuleHandle("WeaponAmmo", _weaponAmmoPrefab, _weaponHolder));
                if (_weaponNamePrefab != null)
                    modules.Add(new HUDModuleHandle("WeaponName", _weaponNamePrefab, _weaponHolder));
            }

            return modules;
        }
    }
}
