using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


namespace Extensions.Utils
{
   [Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    [HorizontalGroup("Split", 0.5f)]
    [LabelWidth(40)]
    public TKey Key;

    [HorizontalGroup("Split", 0.5f)]
    [LabelWidth(40)]
    public TValue Value;

    public SerializableKeyValuePair() { }

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

[Serializable]
public class SerializedDict<TKey, TValue> : ISerializationCallbackReceiver
{
    // Serialized list shown in Inspector
    [TableList(AlwaysExpanded = true)]
    [OdinSerialize]
    [InlineProperty]
    [ValidateInput(nameof(ValidateNoDuplicateKeys), "Duplicate keys detected!")]
    private List<SerializableKeyValuePair<TKey, TValue>> keyValuePairs = new();

    // Runtime dictionary for fast lookup
    private Dictionary<TKey, TValue> dictionary = new();

    // Expose read-only dictionary
    public IReadOnlyDictionary<TKey, TValue> Dictionary => dictionary;

    // Event invoked on dictionary change (optional)
    public event Action OnDictionaryChanged;

    #region Runtime API

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set
        {
            dictionary[key] = value;
            SyncListWithDictionary();
            OnDictionaryChanged?.Invoke();
        }
    }

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public void Add(TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
            throw new ArgumentException($"Key {key} already exists!");

        dictionary.Add(key, value);
        SyncListWithDictionary();
        OnDictionaryChanged?.Invoke();
    }

    public bool Remove(TKey key)
    {
        bool removed = dictionary.Remove(key);
        if (removed)
        {
            SyncListWithDictionary();
            OnDictionaryChanged?.Invoke();
        }
        return removed;
    }

    public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);

    public void Clear()
    {
        dictionary.Clear();
        keyValuePairs.Clear();
        OnDictionaryChanged?.Invoke();
    }

    #endregion

    #region Sync Logic

    private void SyncListWithDictionary()
    {
        keyValuePairs.Clear();
        foreach (var kv in dictionary)
        {
            keyValuePairs.Add(new SerializableKeyValuePair<TKey, TValue>(kv.Key, kv.Value));
        }
    }

    private void SyncDictionaryWithList()
    {
        dictionary.Clear();
        foreach (var kv in keyValuePairs)
        {
            if (!dictionary.ContainsKey(kv.Key))
                dictionary[kv.Key] = kv.Value;
        }
    }

    #endregion

    #region Validation

    private bool ValidateNoDuplicateKeys(List<SerializableKeyValuePair<TKey, TValue>> list)
    {
        if (list == null) return true;

        var keys = new HashSet<TKey>();
        foreach (var kv in list)
        {
            if (kv == null) continue;
            if (kv.Key == null) continue;
            if (!keys.Add(kv.Key))
                return false; // duplicate found
        }
        return true;
    }

    #endregion

    #region Serialization Callbacks

    public void OnBeforeSerialize()
    {
        // Sync list before serialization so data saved to disk is current
        SyncListWithDictionary();
    }

    public void OnAfterDeserialize()
    {
        // Sync dictionary after deserialization so runtime dictionary is ready to use
        SyncDictionaryWithList();
    }

    #endregion

    #region Debugging (Odin Buttons)

    [Button(ButtonSizes.Small)]
    private void DebugPrint()
    {
        Debug.Log($"Dictionary contents ({dictionary.Count} entries):");
        foreach (var kv in dictionary)
        {
            Debug.Log($"  {kv.Key} = {kv.Value}");
        }
    }

    #endregion
}
}