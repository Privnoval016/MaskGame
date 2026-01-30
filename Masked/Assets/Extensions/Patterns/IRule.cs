
namespace Extensions.Patterns
{
    /**
     * <summary>
     * Represents a rule that can be applied to an event and context to produce a result.
     * </summary>
     *
     * <typeparam name="TEvent">The type of the event data.</typeparam>
     * <typeparam name="TContext">The type of the context in which the rule is
     * applied.</typeparam>
     * <typeparam name="TResult">The type of the result produced by applying the rule.</typeparam>
     */
    public interface IRule<in TEvent, in TContext, out TResult>
    {
        bool IsMatch(TEvent eventData, TContext context);
        TResult Apply(TEvent eventData, TContext context);
    }
}