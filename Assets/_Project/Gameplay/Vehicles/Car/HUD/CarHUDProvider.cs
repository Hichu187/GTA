using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Car
{
    public class CarHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _speedoPrefab;
        [SerializeField] private GameObject _gearPrefab;

        public ICarStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("CarSpeedo", _speedoPrefab, StatsSource),
            new HUDModuleHandle("Gear",      _gearPrefab,   StatsSource),
        };
    }
}
