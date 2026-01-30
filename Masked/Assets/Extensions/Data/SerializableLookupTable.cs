using System;
using System.Collections.Generic;

namespace Extensions.Data
{
    [Serializable]
    public abstract class SerializableLookupTable<TKey, TValue>
    {
        [Serializable]
        public struct Entry
        {
            public TKey key;
            public TValue value;
        }

        public List<Entry> entries = new();
        
        private Dictionary<TKey, TValue> _dict;
        private int _cachedHash;

        private int ComputeHash()
        {
            unchecked
            {
                int hash = entries.Count;
                foreach (var e in entries)
                {
                    hash = (hash * 397) ^ (e.key?.GetHashCode() ?? 0);
                    hash = (hash * 397) ^ (e.value?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        private void EnsureCache()
        {
            int newHash = ComputeHash();
            if (_dict == null || newHash != _cachedHash)
            {
                RebuildDictionary();
                _cachedHash = newHash;
            }
        }

        private void RebuildDictionary()
        {
            _dict = new Dictionary<TKey, TValue>(entries.Count);
            foreach (var e in entries)
            {
                _dict[e.key] = e.value;
            }
        }

        /**
         * <summary>
         * Tries to get the value associated with the specified key.
         * </summary>
         *
         * <param name="key">The key to look up.</param>
         * <param name="value">When this method returns, contains the value associated with the
         * specified key, if the key is found; otherwise, the default value for the type
         * of the value parameter. This parameter is passed uninitialized.</param>
         * <returns>true if the LookupTable contains an element with the specified key; otherwise, false.</returns>
         */
        public bool TryGetValue(TKey key, out TValue value)
        {
            EnsureCache();
            return _dict.TryGetValue(key, out value);
        }
        
        /**
         * <summary>
         * Tries to get the key associated with the specified value.
         * </summary>
         *
         * <param name="value">The value to look up.</param>
         * <param name="key">When this method returns, contains the key associated with the
         * specified value, if the value is found; otherwise, the default value for the type
         * of the key parameter. This parameter is passed uninitialized.</param>
         * <returns>true if the LookupTable contains an element with the specified value; otherwise, false.</returns>
         */
        public bool TryGetKey(TValue value, out TKey key)
        {
            EnsureCache();
            foreach (var kvp in _dict)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                {
                    key = kvp.Key;
                    return true;
                }
            }

            key = default;
            return false;
        }

        /**
         * <summary>
         * Gets the value associated with the specified key.
         * </summary>
         *
         * <param name="key">The key to look up.</param>
         * <returns>The value associated with the specified key.</returns>
         * <exception cref="KeyNotFoundException">Thrown if the key is not found in the LookupTable.</exception>
         */
        public TValue GetValue(TKey key)
        {
            EnsureCache();
            return _dict[key];
        }
        
        /**
         * <summary>
         * Gets the key associated with the specified value.
         * </summary>
         *
         * <param name="value">The value to look up.</param>
         * <returns>The key associated with the specified value.</returns>
         * <exception cref="KeyNotFoundException">Thrown if the value is not found in the LookupTable.</exception>
         */
        public TKey GetKey(TValue value)
        {
            EnsureCache();
            foreach (var kvp in _dict)
            {
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                {
                    return kvp.Key;
                }
            }

            throw new KeyNotFoundException("The specified value was not found in the LookupTable.");
        }

        /**
         * <summary>
         * Gets the default value (the value of the first entry) in the LookupTable.
         * </summary>
         *
         * <returns>The default value if the table has entries; otherwise, the default value of TValue.</returns>
         */
        public TValue GetDefaultValue()
        {
            return entries.Count > 0 ? entries[0].value : default;
        }
        
        /**
         * <summary>
         * Gets the default key (the key of the first entry) in the LookupTable.
         * </summary>
         *
         * <returns>The default key if the table has entries; otherwise, the default value of TKey.</returns>
         */
        public TKey GetDefaultKey(TValue value) 
        {
            return entries.Count > 0 ? entries[0].key : default;
        }
    }
}