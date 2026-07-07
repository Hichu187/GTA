using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Rocket
{
    public class RocketHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _throttlePrefab;

        public IRocketStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("RocketSpeedo",   _speedoPrefab,   StatsSource),
            new HUDModuleHandle("RocketAltitude", _altitudePrefab, StatsSource),
            new HUDModuleHandle("RocketThrottle", _throttlePrefab, StatsSource),
        };
    }
}
