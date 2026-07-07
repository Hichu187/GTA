using System;

namespace Game.Systems.Persistence
{
    /// <summary>Root object written to disk. JsonUtility serializes this directly.</summary>
    [Serializable]
    public class SaveFile
    {
        public int    version   = CurrentVersion;
        public string sceneName;
        public string timestamp;
        public SaveEntry[] entries;

        public const int CurrentVersion = 1;
    }

    /// <summary>One ISaveable's serialized state stored as a JSON sub-string.</summary>
    [Serializable]
    public class SaveEntry
    {
        public string key;
        public string data;
    }
}
