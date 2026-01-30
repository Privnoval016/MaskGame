using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.UtilityAI.ConsiderationBases
{
    /**
     * <summary>
     * A consideration that combines multiple other considerations using specified aggregation operations.
     * This allows for complex decision-making by evaluating several factors and combining their scores.
     * </summary>
     */
    [CreateAssetMenu(fileName = "CompositeConsideration", menuName = "UtilityAI/Considerations/CompositeConsideration", order = 0)]
    public class CompositeConsideration : Consideration
    {
        public enum AggregationType
        {
            Average,
            Multiply,
            Add,
            Subtract,
            Divide,
            Max,
            Min
        }
        
        public bool allMustBeNonZero = false;
        public Consideration firstConsideration;
        
        public List<ConsiderationOperation> considerations = new();

        public override float Evaluate(IContextBase context)
        {
            if (firstConsideration == null) return 0f;
            
            float score = firstConsideration.Evaluate(context);
            if (allMustBeNonZero && score == 0f) return 0f;

            foreach (var considerationOperation in considerations)
            {
                var consideration = considerationOperation.consideration;
                var operation = considerationOperation.operation;
                
                if (consideration == null) continue;
                
                float considerationScore = consideration.Evaluate(context);
                if (allMustBeNonZero && considerationScore == 0f) return 0f;

                switch (operation)
                {
                    case AggregationType.Average:
                        score = (score + considerationScore) / 2f;
                        break;
                    case AggregationType.Multiply:
                        score *= considerationScore;
                        break;
                    case AggregationType.Add:
                        score += considerationScore;
                        break;
                    case AggregationType.Subtract:
                        score -= considerationScore;
                        break;
                    case AggregationType.Divide:
                        if (considerationScore != 0f)
                            score /= considerationScore;
                        else
                            score = 0f; // Avoid division by zero
                        break;
                    case AggregationType.Max:
                        score = Mathf.Max(score, considerationScore);
                        break;
                    case AggregationType.Min:
                        score = Mathf.Min(score, considerationScore);
                        break;
                }
            }

            return Mathf.Clamp01(score);
        }
        
        [Serializable]
        public struct ConsiderationOperation
        {
            public AggregationType operation;
            public Consideration consideration;
        }
    }
}