using System.Collections.Generic;

namespace Extensions.Patterns
{
    public interface IRuleBucket { }
    
    public class RuleBucket<TEvent, TContext, TResult> : IRuleBucket
    {
        public readonly List<IRule<TEvent, TContext, TResult>> Rules = new List<IRule<TEvent, TContext, TResult>>();
    }
    
    internal static class RuleBucketExtensions
    {
        public static RuleBucket<TEvent, TContext, TResult> As<TEvent, TContext, TResult>(
            this IRuleBucket bucket)
        {
            return (RuleBucket<TEvent, TContext, TResult>)bucket;
        }
    }
}