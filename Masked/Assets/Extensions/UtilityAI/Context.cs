using System.Collections.Generic;
using UnityEngine;

namespace Extensions.UtilityAI
{
    /**
     * <summary>
     * Context class for storing and retrieving data using a generic key type.
     * This class is used to provide context to AI actions and considerations.
     * </summary>
     *
     * <typeparam name="TKey">The type used as a key to identify context data.</typeparam>
     */
    public class Context<TKey> : IContextBase
    {
        public AIBrain<TKey> AIBrain;
        public Sensor<TKey> Sensor;
        
        private readonly Dictionary<TKey, object> data = new();
        
        public Context(AIBrain<TKey> aiBrain)
        {
            AIBrain = aiBrain;
            Sensor = aiBrain.User.GetSensor();
        }

        public TValue GetData<TValue>(object key)
        {
            if (key is not TKey keyEnum) return default;
            
            return data.GetValueOrDefault(keyEnum) is TValue value ? value : default;
        }

        public bool SetData<TValue>(object key, TValue value)
        {
            if (key is not TKey keyEnum) return false;
            
            data[keyEnum] = value;
            return true;
        }
        
        public Transform GetSensorTarget(object key)
        {
            if (key is not TKey keyEnum) return null;

            return Sensor.GetNearestDetectedObject(keyEnum);
        }
        
        public Transform GetBrainTransform()
        {
            return AIBrain.User.transform;
        }
    }

    /**
     * <summary>
     * Interface for context classes to get and set data using generic keys.
     * Allows for flexible and type-safe access to context information.
     * </summary>
     */
    public interface IContextBase
    {
        public TValue GetData<TValue>(object key);
        public bool SetData<TValue>(object key, TValue value);

        public Transform GetSensorTarget(object key);

        public Transform GetBrainTransform();
    }
}