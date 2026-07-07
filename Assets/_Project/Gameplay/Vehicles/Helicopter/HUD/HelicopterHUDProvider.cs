using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public class HelicopterHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _verticalSpeedPrefab;

        public IHelicopterStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("HeliSpeedo",        _speedoPrefab,        StatsSource),
            new HUDModuleHandle("HeliAltitude",      _altitudePrefab,      StatsSource),
            new HUDModuleHandle("HeliVerticalSpeed",  _verticalSpeedPrefab, StatsSource),
        };
    }
}
