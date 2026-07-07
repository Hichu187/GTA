using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Vehicles.Common.Stubs
{
    public class DummyVehicleHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        // Phase 2: empty — real Vehicle HUD modules added per-vehicle from Phase 5.
        private static readonly List<HUDModuleHandle> _empty = new();

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => _empty;
    }
}
