using Extensions.UtilityAI.ConsiderationBases;
using UnityEngine;

namespace Extensions.UtilityAI
{
    /**
     * <summary>
     * Base class for AI actions that can be evaluated and executed based on a utility score.
     * Each action has an associated consideration that determines its utility based on the current context.
     * The action with the highest utility is selected for execution.
     * </summary>
     * 
     * <typeparam name="TKey">The type used as a key to identify the action.</typeparam>
     *
     */
    public abstract class AIAction<TKey> : ScriptableObject
    {
        [Header("General Settings")]
        public TKey actionKey;
        public Consideration consideration;
        
        public float CalculateUtility(Context<TKey> context)
        {
            if (consideration == null) return 1f;
            return Mathf.Clamp01(consideration.Evaluate(context));
        }
    }
}