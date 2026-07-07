using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Gameplay.Character.Stubs
{
    public class CharacterStubHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        // Phase 2: empty — HUD modules added in Phase 3.
        private static readonly List<HUDModuleHandle> _empty = new();

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => _empty;
    }
}
