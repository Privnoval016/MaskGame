using System;
using UnityEngine;

namespace Extensions.UtilityAI.ConsiderationBases
{
    
    /**
     * <summary>
     * Abstract base class for defining considerations in a utility AI system.
     * Considerations evaluate the current context and return a utility score between 0 and 1
     * that indicates the desirability of a particular action or decision.
     * </summary>
     */
    public abstract class Consideration : ScriptableObject
    {
        public abstract float Evaluate(IContextBase context);
    }

    /**
     * <summary>
     * Interface for defining keys used to retrieve data from the AI context.
     * Override with specific implementations to provide type-safe keys that can be assigned in the Unity Inspector.
     * </summary>
     */
    public abstract class ConsiderationKey
    {
        public abstract object GetKey(out Type enumType);
    }
}
