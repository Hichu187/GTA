using UnityEngine;

namespace Game.Core.HUD
{
    public readonly struct HUDModuleHandle
    {
        public readonly string     ModuleId;
        public readonly GameObject ModulePrefab;
        public readonly object     DataSource;

        public HUDModuleHandle(string moduleId, GameObject modulePrefab, object dataSource = null)
        {
            ModuleId     = moduleId;
            ModulePrefab = modulePrefab;
            DataSource   = dataSource;
        }

        public bool IsValid => ModulePrefab != null;
    }
}
