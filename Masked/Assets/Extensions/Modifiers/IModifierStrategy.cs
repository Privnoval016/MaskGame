
namespace Extensions.Modifiers
{
    public abstract class IModifierStrategy
    {
        public abstract (int, int) Modify(int baseValue, int currentValue); // Method to modify the value
    
        public override bool Equals(object obj)
        {
            return obj?.GetType() == GetType();
        }
    
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }
    
        public static bool operator ==(IModifierStrategy left, IModifierStrategy right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }
    
        public static bool operator !=(IModifierStrategy left, IModifierStrategy right)
        {
            return !(left == right);
        }
    }
}