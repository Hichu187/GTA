using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    public class MotorcycleHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _rpmPrefab;

        public IMotorcycleStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("Speedo", _speedoPrefab, StatsSource),
            new HUDModuleHandle("RPM",    _rpmPrefab,    StatsSource),
        };
    }
}
