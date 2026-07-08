using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Rocket
{
    public class RocketHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _throttlePrefab;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPrefab;

        public IRocketStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("RocketSpeedo",   _speedoPrefab,   StatsSource),
                new HUDModuleHandle("RocketAltitude", _altitudePrefab, StatsSource),
                new HUDModuleHandle("RocketThrottle", _throttlePrefab, StatsSource),
            };
            if (_mobileControlsPrefab != null)
                modules.Add(new HUDModuleHandle("MobileControls", _mobileControlsPrefab, GetComponent<ILookInjectable>()));
            return modules;
        }
    }
}
