using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extensions.Persistence
{
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem>
    {
        [SerializeField] public GameData gameData;
        
        private IDataService dataService;

        #region Monobehaviour Callbacks
        protected override void Awake()
        {
            base.Awake();
            dataService = new FileDataService(new JsonSerializer());
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        #endregion
        
        #region Binding Logic
        
        private void Bind<T>(ISaveable saveable) where T : MonoBehaviour, IBind<ISaveable>
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null)
            {
                entity.Bind(saveable);
            }
        }

        private void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null)
            {
                if (data != null) {
                    data = new TData { Id = entity.Id };
                }
                entity.Bind(data);
            }
        }

        private void Bind<T, TData>(List<TData> data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new()
        {
            var entities = FindObjectsByType<T>(FindObjectsSortMode.None);
            
            foreach (var entity in entities) 
            {
                var entityData = data.FirstOrDefault(d => d.Id == entity.Id);
                if (entityData == null) 
                {
                    entityData = new TData { Id = entity.Id };
                    data.Add(entityData);
                }
                entity.Bind(entityData);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // precautionary binding logic can be placed here if needed

            foreach (ISaveable saveable in gameData.Saveables)
            {
                InvokeBindFor(saveable);
            }
        }
        
        private void InvokeBindFor(ISaveable saveable)
        {
            if (saveable == null) return;
            
            Type bindType = saveable.BindType;
            
            Type saveableType = saveable.GetType();
            
            var bindMethod = typeof(SaveLoadSystem)
                .GetMethod("Bind", BindingFlags.NonPublic | BindingFlags.Instance);

            if (bindMethod != null)
            {
                var genericBindMethod = bindMethod.MakeGenericMethod(bindType, saveableType);
                genericBindMethod.Invoke(this, new object[] { saveable });
            }
        }

        #endregion

        #region Save/Load Methods
        
        public void NewGame()
        {
            
        }
        
        public void SaveGame()
        {
            dataService.Save(gameData, true);
        }

        public void LoadGame(string saveName)
        {
            gameData = dataService.Load(saveName);
        }
        
        public void DeleteGame(string saveName)
        {
            dataService.Delete(saveName);
        }
        
        #endregion
    }
}