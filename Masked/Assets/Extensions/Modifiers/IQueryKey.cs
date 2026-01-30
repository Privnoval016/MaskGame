namespace Extensions.Modifiers
{
    /**
 * <summary>
 * Interface for query types. This is a marker interface used to define different types of queries.
 * </summary>
 */
    public abstract class IQueryKey<T>
    {
        public override bool Equals(object obj)
        {
            return OnEquals(obj);
        }

        public override int GetHashCode()
        {
            return OnGetHashCode();
        }
    
        public static bool operator ==(IQueryKey<T> a, IQueryKey<T> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }
    
        public static bool operator !=(IQueryKey<T> a, IQueryKey<T> b)
        {
            return !(a == b);
        }
    
        protected abstract bool OnEquals(object obj); // Implement a proper equality method
    
        protected abstract int OnGetHashCode(); // Implement a proper hash code method
    }
}