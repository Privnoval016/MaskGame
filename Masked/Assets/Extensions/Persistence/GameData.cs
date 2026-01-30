using System;
using System.Collections.Generic;

namespace Extensions.Persistence
{
    [Serializable]
    public class GameData { 
        public string Name { get; }
        public DateTime LastModified { get; set; }
        public List<ISaveable> Saveables { get; set; } = new();
    }
}