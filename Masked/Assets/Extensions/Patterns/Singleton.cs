using UnityEngine;

namespace Extensions.Patterns
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static bool _isQuitting;

        public static T Instance
        {
            get
            {
                if (_isQuitting) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                    if (_instance == null)
                    {
                        Debug.LogWarning($"No instance of {typeof(T).Name} found in the scene. Please ensure that an instance is present.");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"Duplicate {typeof(T).Name} found on {gameObject.name}, destroying.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            // Avoid clearing the instance if Unity is quitting or domain is reloading
            if (_instance == this && !_isQuitting)
                _instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        // Reset static state after domain reload (Unity 2019.3+)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _isQuitting = false;
        }
    }
}