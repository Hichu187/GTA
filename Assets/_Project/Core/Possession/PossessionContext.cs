using UnityEngine;

namespace Game.Core.Possession
{
    // Keep this struct minimal — it becomes a dependency of every IPossessable.
    // Add a field only when a concrete module cannot function without it.
    // See architecture doc section 16: "PossessionContext as God Object" risk.
    public readonly struct PossessionContext
    {
        /// <summary>Player slot index — 0 for single-player, 0/1 for split-screen.</summary>
        public readonly int PlayerIndex;

        /// <summary>
        /// Where the receiving entity should place itself.
        /// OnPossess: ExitAnchor of the previous entity (character spawns at vehicle door).
        /// OnUnpossess: EnterAnchor of the incoming entity (character sits in vehicle seat).
        /// Null = no relocation needed.
        /// </summary>
        public readonly Transform    AnchorPoint;

        /// <summary>
        /// Callback injected by PossessionManager — vehicle fires this to trigger exit back to previous entity.
        /// Null for non-vehicle entities.
        /// </summary>
        public readonly System.Action OnExitRequested;

        public PossessionContext(int playerIndex,
                                 Transform anchorPoint      = null,
                                 System.Action onExitRequested = null)
        {
            PlayerIndex      = playerIndex;
            AnchorPoint      = anchorPoint;
            OnExitRequested  = onExitRequested;
        }

        public static readonly PossessionContext Default = new PossessionContext(0);
    }
}
