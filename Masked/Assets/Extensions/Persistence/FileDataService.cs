using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Extensions.Persistence
{
    public class FileDataService : IDataService
    {
        private ISerializer serializer;
        string dataPath;
        string fileExtension;

        public FileDataService(ISerializer serializer)
        {
            this.dataPath = Application.persistentDataPath;
            this.fileExtension = "save";
            this.serializer = serializer;
        }
        
        private string GetPathToFile(string name)
        {
            return Path.Combine(dataPath, string.Concat(name, ".", fileExtension));
        }
        
        public void Save(GameData data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.Name);

            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException($"File {data.Name}.{fileExtension} already exists and cannot be overwritten.");
            }
            
            File.WriteAllText(fileLocation, serializer.Serialize(data));
        }

        public GameData Load(string name)
        {
            string fileLocation = GetPathToFile(name);

            if (!File.Exists(fileLocation))
            {
                throw new FileNotFoundException($"File {name}.{fileExtension} not found.");
            }

            string fileContents = File.ReadAllText(fileLocation);
            return serializer.Deserialize<GameData>(fileContents);
        }

        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);
            
            if (File.Exists(fileLocation))
            {
                File.Delete(fileLocation);
            }
        }

        public void DeleteAll()
        {
            foreach (string path in Directory.GetFiles(dataPath, $"*{fileExtension}"))
            {
                File.Delete(path);
            }
        }

        public IEnumerable<string> ListSaves()
        {
            foreach (string path in Directory.EnumerateFiles(dataPath, $"*{fileExtension}"))
            {
                yield return Path.GetFileNameWithoutExtension(path);
            }
        }
    }
}