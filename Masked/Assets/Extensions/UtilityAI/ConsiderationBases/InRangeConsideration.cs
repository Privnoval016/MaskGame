using Extensions.Utils;
using UnityEngine;

namespace Extensions.UtilityAI.ConsiderationBases
{
    /**
     * <summary>
     * A consideration that evaluates how close a target is to the agent, both in distance and angle.
     * Returns a value based on the provided AnimationCurve, where 0 is farthest and 1 is closest.
     * </summary>
     */
    [CreateAssetMenu(fileName = "InRangeConsideration", menuName = "UtilityAI/Considerations/InRangeConsideration", order = 0)]
    public class InRangeConsideration : Consideration
    {
        public float maxDistance = 10f;
        public float maxAngle = 360f;
        [SerializeReference] public ConsiderationKey targetKey;
        [Tooltip("An animation curve that defines the score based on normalized distance (0 = closest, 1 = farthest).")]
        public AnimationCurve curve;

        public override float Evaluate(IContextBase context)
        {
            var target = context.GetSensorTarget(targetKey.GetKey(out var enumType));
            if (target == null) return 0f;
            
            bool isInRange = context.GetBrainTransform().forward.IsInDirectionCone(
                target.position - context.GetBrainTransform().position, maxAngle) &&
                             Vector3.Distance(context.GetBrainTransform().position, target.position) <= maxDistance;
            
            if (!isInRange) return 0f;
            
            Vector3 directionToTarget = (target.position - context.GetBrainTransform().position).normalized;
            float distanceToTarget = directionToTarget.ZeroVector3Axis().magnitude;
            
            
            float normalizedDistance = Mathf.Clamp01(distanceToTarget / maxDistance);
            
            return Mathf.Clamp01(curve.Evaluate(normalizedDistance));
        }

        void Reset()
        {
            curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        }
    }
}