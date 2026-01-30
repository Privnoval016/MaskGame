using UnityEngine;

namespace Extensions.Patterns
{
    public interface ICommand<T>
    {
        void Execute();
    }
}