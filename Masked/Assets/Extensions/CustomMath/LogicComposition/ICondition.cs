using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Extensions.CustomMath.LogicComposition
{
    /**
     * Interface for defining a condition that can be evaluated against a given context.
     *
     * @tparam TContext The type of context the condition evaluates against.
     */
    public interface ICondition<in TContext>
    {
        bool Evaluate(TContext context);
    }
}