using UnityEngine;

namespace Extensions.UtilityAI.ConsiderationBases
{
    /**
     * <summary>
     * A consideration that always returns a constant value.
     * </summary>
     */
    [CreateAssetMenu(fileName = "ConstantConsideration", menuName = "UtilityAI/Considerations/ConstantConsideration", order = 0)]
    public class ConstantConsideration : Consideration
    {
        public float value;
        
        public override float Evaluate(IContextBase context) => value;

    }
}