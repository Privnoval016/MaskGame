using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions.Modifiers
{
    /**
 * <summary>
 * Mediator class that manages queries and modifiers. It allows adding modifiers that can modify the outcome of
 * queries. The mediator raises events when a query is performed, and all registered modifiers can handle these events
 * to modify the query context.
 * </summary>
 */
    public class Mediator<T> where T : IQueryKey<T>
    {
        private readonly List<Modifier<T>> Modifiers = new();
    
        public Action OnModifiersChanged = delegate { };
    
        public void PerformQuery(object sender, QueryContext<T> queryContext)
        {
        
            foreach (var modifier in Modifiers)
            {
                modifier.Handle(sender, queryContext);
            }
        }
    
        public void AddModifier(Modifier<T> mod)
        {
            Modifiers.Add(mod);
            Modifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        
            mod.MarkedForRemoval = false;

            mod.OnDisposed += _ =>
            {
                Modifiers.Remove(mod);
                OnModifiersChanged?.Invoke();
            };
        
            OnModifiersChanged?.Invoke();
        }

        public void Update()
        {
            foreach (var mod in Modifiers)
            {
                mod.Update();
            }
        
            foreach (var mod in Modifiers.Where(m => m.MarkedForRemoval).ToList())
            {
                mod.Dispose();
            }
        }
    }
}