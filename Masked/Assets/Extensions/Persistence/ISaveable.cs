using System;
using Extensions.Serialization;
using UnityEngine;

namespace Extensions.Persistence
{
    public interface ISaveable
    {
        SerializableGuid Id { get; set; } // Unique identifier for the owner of the saveable data
        Type BindType { get; } // The type of the bindable entity
    }
    
    public abstract class SaveableBase<TBind> : ISaveable where TBind : MonoBehaviour
    {
        public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
        public Type BindType => typeof(TBind);
    }
    
    public interface IBind<in TData> where TData : ISaveable
    {
        SerializableGuid Id { get; set; } // Unique identifier for the bindable data
        void Bind(TData data);
    }
}