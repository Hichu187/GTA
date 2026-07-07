namespace Game.Core.Persistence
{
    /// <summary>
    /// Implement on any MonoBehaviour whose state must survive a save/load cycle.
    /// Each implementor owns its own serialization format — use JsonUtility internally.
    /// SaveService discovers implementors via registration, not scene scanning.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Unique, stable identifier — matches the key stored in the save file.</summary>
        string SaveKey { get; }

        /// <summary>Return a JSON string representing the current state.</summary>
        string CaptureState();

        /// <summary>Restore state from a JSON string produced by a prior CaptureState call.</summary>
        void RestoreState(string json);
    }
}
