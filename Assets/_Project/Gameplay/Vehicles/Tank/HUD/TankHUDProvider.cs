using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Tank
{
    public class TankHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _ammoPrefab;
        [SerializeField] private GameObject _crosshairPrefab;
        [SerializeField] private GameObject _fireCooldownPrefab;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPrefab;

        public ITankStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("TankSpeedo",      _speedoPrefab,      StatsSource),
                new HUDModuleHandle("TankAmmo",        _ammoPrefab,        StatsSource),
                new HUDModuleHandle("TankCrosshair",   _crosshairPrefab,   null),
                new HUDModuleHandle("TankFireCooldown", _fireCooldownPrefab, StatsSource),
            };
            if (_mobileControlsPrefab != null)
                modules.Add(new HUDModuleHandle("MobileControls", _mobileControlsPrefab,
                    GetComponent<ILookInjectable>()));
            return modules;
        }
    }
}
