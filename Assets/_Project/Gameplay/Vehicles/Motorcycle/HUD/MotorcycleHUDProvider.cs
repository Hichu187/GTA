using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    public class MotorcycleHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _rpmPrefab;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPrefab;

        public IMotorcycleStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("Speedo", _speedoPrefab, StatsSource),
                new HUDModuleHandle("RPM",    _rpmPrefab,    StatsSource),
            };
            if (_mobileControlsPrefab != null)
                modules.Add(new HUDModuleHandle("MobileControls", _mobileControlsPrefab, GetComponent<ILookInjectable>()));
            return modules;
        }
    }
}
