using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Airplane
{
    public class AirplaneHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _headingPrefab;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPrefab;

        public IAirplaneStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("AirSpeedo", _speedoPrefab,   StatsSource),
                new HUDModuleHandle("Altitude",  _altitudePrefab, StatsSource),
                new HUDModuleHandle("Heading",   _headingPrefab,  StatsSource),
            };
            if (_mobileControlsPrefab != null)
                modules.Add(new HUDModuleHandle("MobileControls", _mobileControlsPrefab, GetComponent<ILookInjectable>()));
            return modules;
        }
    }
}
