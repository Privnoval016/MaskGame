using System;
using System.Collections.Generic;
using Extensions.EventBus;
using Extensions.Timers;
using UnityEngine;

namespace Extensions.UtilityAI
{
    /**
     * <summary>
     * AIBrain class that manages AI actions and context updates.
     * It periodically updates the context and calculates the best action to execute based on utility scores.
     * </summary>
     *
     * <typeparam name="TKey">The type used as a key to identify context data.</typeparam>
     */
    public class AIBrain<TKey>
    {
        public Context<TKey> Context;
        public AIBrainUser<TKey> User;

        public AIBrain(AIBrainUser<TKey> user)
        {
            User = user;
        }

        public void Initialize()
        {
            Context = new Context<TKey>(this);
        }

        public void UpdateContext()
        {
            ContextPayload<TKey>[] payloads = User.OnContextUpdate();
            
            foreach (var payload in payloads)
            {
                Context.SetData(payload.Key, payload.Value);
            }
        }

        public void CalculateBestAction()
        {
            AIAction<TKey> bestAction = null;
            float highestUtility = float.MinValue;

            foreach (var action in User.GetActions())
            {
                float utility = action.CalculateUtility(Context);
                if (utility > highestUtility)
                {
                    highestUtility = utility;
                    bestAction = action;
                }
            }

            if (bestAction != null)
            {
                User.ExecuteNewAction(bestAction, Context, highestUtility);
            }
        }
    }

    /**
     * <summary>
     * Interface for classes that use an AIBrain to manage AI actions and context updates.
     * Implementing classes must provide methods to retrieve the AIBrain instance and handle context updates
     * by returning an array of ContextPayload objects.
     * </summary>
     */
    public abstract class AIBrainUser<TKey> : MonoBehaviour
    {
        public abstract List<AIAction<TKey>> GetActions();

        public abstract Sensor<TKey> GetSensor();

        public abstract ContextPayload<TKey>[] OnContextUpdate();
        
        public abstract void ExecuteNewAction(AIAction<TKey> action, Context<TKey> context, float highestUtility);
    }

    public struct ContextPayload<TKey>
    {
        public TKey Key;
        public object Value;
        
        public ContextPayload(TKey key, object value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator ContextPayload<TKey>((TKey key, object value) tuple)
        {
            return new ContextPayload<TKey>(tuple.key, tuple.value);
        }
    }
}