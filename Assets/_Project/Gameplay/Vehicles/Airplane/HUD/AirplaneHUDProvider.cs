using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Airplane
{
    public class AirplaneHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _headingPrefab;

        public IAirplaneStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("AirSpeedo",  _speedoPrefab,   StatsSource),
            new HUDModuleHandle("Altitude",   _altitudePrefab, StatsSource),
            new HUDModuleHandle("Heading",    _headingPrefab,  StatsSource),
        };
    }
}
