namespace Extensions.Modifiers
{
    /**
 * <summary>
 * Context for a query, containing the key and value. This is an important wrapper class because it allows us to
 * abstract away the key of the query, letting us reuse the same Mediator and Modifier classes for different types
 * of queries (e.g., stats, status effects) without needing to create separate classes for each type.
 * </summary>
 */
    public class QueryContext<TQueryKey> where TQueryKey : IQueryKey<TQueryKey>
    {
        public TQueryKey Key;
        public int BaseValue;
        public int CurrentValue;
    
        public QueryContext(TQueryKey key, int baseValue, int currentValue)
        {
            Key = key;
            BaseValue = baseValue;
            CurrentValue = currentValue;
        }
    }
}