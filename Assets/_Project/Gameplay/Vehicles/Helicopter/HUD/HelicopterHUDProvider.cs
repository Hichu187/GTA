using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public class HelicopterHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _verticalSpeedPrefab;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject _mobileControlsPrefab;

        public IHelicopterStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules()
        {
            var modules = new List<HUDModuleHandle>
            {
                new HUDModuleHandle("HeliSpeedo",       _speedoPrefab,        StatsSource),
                new HUDModuleHandle("HeliAltitude",     _altitudePrefab,      StatsSource),
                new HUDModuleHandle("HeliVerticalSpeed", _verticalSpeedPrefab, StatsSource),
            };
            if (_mobileControlsPrefab != null)
                modules.Add(new HUDModuleHandle("MobileControls", _mobileControlsPrefab, GetComponent<ILookInjectable>()));
            return modules;
        }
    }
}
