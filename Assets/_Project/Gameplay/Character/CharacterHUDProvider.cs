using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;
using Game.Gameplay.Character.Stats;

namespace Game.Gameplay.Character
{
    public class CharacterHUDProvider : MonoBehaviour, IHUDContextProvider
    {
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private GameObject _staminaBarPrefab;
        [SerializeField] private GameObject _crosshairPrefab;

        public ICharacterStats StatsSource { get; set; }

        public IReadOnlyList<HUDModuleHandle> GetActiveHUDModules() => new List<HUDModuleHandle>
        {
            new HUDModuleHandle("HealthBar",  _healthBarPrefab,  StatsSource),
            new HUDModuleHandle("StaminaBar", _staminaBarPrefab, StatsSource),
            new HUDModuleHandle("Crosshair",  _crosshairPrefab,  null),
        };
    }
}
