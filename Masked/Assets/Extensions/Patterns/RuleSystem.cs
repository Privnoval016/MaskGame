using System;
using System.Collections.Generic;

namespace Extensions.Patterns
{
    public class RuleSystem<TContext, TResult>
    {
        private readonly Dictionary<Type, IRuleBucket> _buckets = new Dictionary<Type, IRuleBucket>();
        
        public void AddRule<TEvent>(IRule<TEvent, TContext, TResult> rule)
        {
            var eventType = typeof(TEvent);
            if (!_buckets.TryGetValue(eventType, out var bucket))
            {
                bucket = new RuleBucket<TEvent, TContext, TResult>();
                _buckets[eventType] = bucket;
            }

            bucket.As<TEvent, TContext, TResult>().Rules.Add(rule);
        }
        public TResult ApplyRules<TEvent>(TEvent eventData, TContext context, Func<TResult, TResult, TResult> combiner, 
            IRule<TEvent, TContext, TResult> firstRule = null,
            IEnumerable<IRule<TEvent, TContext, TResult>> additionalRules = null,
            IRule<TEvent, TContext, TResult> finalRule = null)
        {
            var eventType = typeof(TEvent);
            if (!_buckets.TryGetValue(eventType, out var bucket))
            {
                bucket = new RuleBucket<TEvent, TContext, TResult>();
            }
            
            var ruleBucket = bucket.As<TEvent, TContext, TResult>();
            TResult result = default;
            bool initialized = false;
            
            // Apply first rule if provided
            if (firstRule != null && firstRule.IsMatch(eventData, context))
            {
                var ruleResult = firstRule.Apply(eventData, context);
                
                result = ruleResult;
                initialized = true;
            }
            
            // Apply rules from the bucket
            foreach (var rule in ruleBucket.Rules)
            {
                if (!rule.IsMatch(eventData, context)) continue;
                
                var ruleResult = rule.Apply(eventData, context);
                
                if (!initialized)
                {
                    result = ruleResult;
                    initialized = true;
                }
                else
                {
                    result = combiner(result, ruleResult);
                }
            }
            
            // Apply additional rules
            if (additionalRules != null)
            {
                foreach (var rule in additionalRules)
                {
                    if (!rule.IsMatch(eventData, context)) continue;

                    var ruleResult = rule.Apply(eventData, context);

                    if (!initialized)
                    {
                        result = ruleResult;
                        initialized = true;
                    }
                    else
                    {
                        result = combiner(result, ruleResult);
                    }
                }
            }

            // Apply final rule if provided
            if (finalRule != null && finalRule.IsMatch(eventData, context))
            {
                var ruleResult = finalRule.Apply(eventData, context);

                if (!initialized)
                {
                    result = ruleResult;
                    initialized = true;
                }
                else
                {
                    result = combiner(result, ruleResult);
                }
            }
            
            return result;
        }
    }
}