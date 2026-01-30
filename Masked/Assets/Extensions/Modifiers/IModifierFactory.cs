namespace Extensions.Modifiers
{
    public interface IModifierFactory<T> where T : IQueryKey<T>
    {
        Modifier<T> Create(IModifierProvider modifier);
    }
}