using Sirenix.OdinInspector;
using UnityEngine;

namespace Extensions.UtilityAI.ConsiderationBases
{
    /**
     * <summary>
     * A consideration that returns a random value between a specified min and max.
     * </summary>
     */
    [CreateAssetMenu(fileName = "RandomConsideration", menuName = "UtilityAI/Considerations/RandomConsideration", order = 0)]
    public class RandomConsideration : Consideration
    {
        [MinMaxSlider(0, 1)]
        public Vector2 minMax = new Vector2(0, 1);
        
        public override float Evaluate(IContextBase context)
        {
            return UnityEngine.Random.Range(minMax.x, minMax.y);
        }
    }
}