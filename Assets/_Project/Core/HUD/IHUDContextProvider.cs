using System.Collections.Generic;

namespace Game.Core.HUD
{
    public interface IHUDContextProvider
    {
        IReadOnlyList<HUDModuleHandle> GetActiveHUDModules();
    }
}
