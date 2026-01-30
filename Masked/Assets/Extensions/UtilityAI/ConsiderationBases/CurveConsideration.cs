using UnityEngine;

namespace Extensions.UtilityAI.ConsiderationBases
{
    /**
     * <summary>
     * A consideration that evaluates an input value using a customizable animation curve.
     * The curve maps input values (typically between 0 and 1) to output utility scores (also between 0 and 1).
     * This allows for fine-tuning how different input values influence the utility score.
     * The input value is retrieved from the AI's context using a specified key.
     * </summary>
     */
    [CreateAssetMenu(fileName = "CurveConsideration", menuName = "UtilityAI/Considerations/CurveConsideration", order = 0)]
    public class CurveConsideration : Consideration
    {
        public AnimationCurve curve;
        [SerializeReference] public ConsiderationKey contextKey;
        
        public override float Evaluate(IContextBase context)
        {
            float inputValue = context.GetData<float>(contextKey.GetKey(out var enumType));
            
            return Mathf.Clamp01(curve.Evaluate(inputValue));
        }

        private void Reset()
        {
            curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        }
    }
}