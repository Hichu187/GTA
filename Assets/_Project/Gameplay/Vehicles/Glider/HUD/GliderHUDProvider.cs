using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Glider
{
    public class GliderHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _altitudePrefab;
        [SerializeField] private GameObject _verticalSpeedPrefab;

        public IGliderStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("GliderSpeedo",       _speedoPrefab,        StatsSource),
            new HUDModuleHandle("GliderAltitude",     _altitudePrefab,      StatsSource),
            new HUDModuleHandle("GliderVerticalSpeed", _verticalSpeedPrefab, StatsSource),
        };
    }
}
