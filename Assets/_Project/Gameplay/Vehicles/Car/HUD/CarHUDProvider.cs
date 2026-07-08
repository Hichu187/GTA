using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Car
{
    public class CarHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _gearPrefab;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPrefab;

        public ICarStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("CarSpeedo", _speedoPrefab, StatsSource),
                new HUDModuleHandle("Gear",      _gearPrefab,   StatsSource),
            };
            if (_mobileControlsPrefab != null)
                modules.Add(new HUDModuleHandle("MobileControls", _mobileControlsPrefab, GetComponent<ILookInjectable>()));
            return modules;
        }
    }
}
