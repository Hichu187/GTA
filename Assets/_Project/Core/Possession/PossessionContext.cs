namespace Game.Core.Possession
{
    // Keep this struct minimal — it becomes a dependency of every IPossessable.
    // Add a field only when a concrete module cannot function without it.
    // See architecture doc section 16: "PossessionContext as God Object" risk.
    public readonly struct PossessionContext
    {
        /// <summary>Player slot index — 0 for single-player, 0/1 for split-screen.</summary>
        public readonly int PlayerIndex;

        public PossessionContext(int playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public static readonly PossessionContext Default = new PossessionContext(0);
    }
}
