using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NovelEngine.Scripts
{
    [Serializable]
    public class SerializableDict<TKey, TValue> : ISerializationCallbackReceiver
    {
        public List<SerializableKeyValuePair<TKey, TValue>> pairs = new();
    
        [SerializeField, HideInInspector] public List<TKey> keys = new();
        [SerializeField, HideInInspector] public List<TValue> values = new();

        public IReadOnlyList<TKey> Keys
        {
            get
            {
                var result = new List<TKey>(pairs.Count);
                foreach(var pair in pairs)
                    result.Add(pair.key);
                keys = result;
                return result;
            }
        }
    
        public IReadOnlyList<TValue> Values
        {
            get
            {
                var result = new List<TValue>(pairs.Count);
                foreach(var pair in pairs)
                    result.Add(pair.value);
                values = result;
                return result;
            }
        }
    
    
        public int Count => pairs.Count;

        [NonSerialized] private Dictionary<TKey, TValue> _runtimeDict;

        public void Add(TKey key, TValue value)
        {
            if(ContainsKey(key))
                throw new ArgumentException($"key already exists: {key}");
        
            pairs.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
            keys.Add(key);
            values.Add(value);
        }
    
        public TValue GetValue(TKey key)
        {
            foreach (var pair in pairs)
            {
                if (EqualityComparer<TKey>.Default.Equals(pair.key, key))
                    return pair.value;
            }
            return default;
        }

        public TValue this[TKey key]
        {
            get => GetValue(key);
            set
            {
                for (int i = 0; i < pairs.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(pairs[i].key, key))
                    {
                        pairs[i].value = value;
                        values[i] = value;
                        return;
                    }
                }

                Add(key, value);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            foreach (var pair in pairs)
            {
                if (EqualityComparer<TKey>.Default.Equals(pair.key, key))
                {
                    value = pair.value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    
        public bool ContainsKey(TKey key)
        {
            foreach (var pair in pairs)
            {
                if (EqualityComparer<TKey>.Default.Equals(pair.key, key))
                    return true;
            }
            return false;
        }
    
        public bool Remove(TKey key)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(pairs[i].key, key))
                {
                    pairs.RemoveAt(i);
                    keys.RemoveAt(i);
                    values.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    
        public void ChangeKey(TKey originKey, TKey newKey)
        {
            if (EqualityComparer<TKey>.Default.Equals(originKey, newKey))
                return;


            TryGetValue(originKey, out var value);

            if (value != null)
            {
                Add(newKey, value);
                Remove(originKey);
            }
            else
            {
                Debug.LogError($"키 {originKey}가 존재하지 않음");
            }
        }
        public void Clear()
        {
            pairs.Clear();
            keys.Clear();
            values.Clear();
            _runtimeDict?.Clear();
        }
    

        #region For Faster Runtime
    
        public void RebuildDictionary()
        {
            _runtimeDict = new Dictionary<TKey, TValue>();
            foreach (var pair in pairs)
            {
                _runtimeDict[pair.key] = pair.value;
            }
        }

        public TValue GetValueFast(TKey key)
        {
            if (_runtimeDict == null)
                RebuildDictionary();

            _runtimeDict.TryGetValue(key, out var val);
            return val;
        }
    
        #endregion
    
        public void OnAfterDeserialize()
        {
            RebuildDictionary();
        }

        public void OnBeforeSerialize()
        { }
    }

    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        [FormerlySerializedAs("key")]
        [SerializeField] private TKey _key;
        public TValue value;
        public TKey key => _key;
        public SerializableKeyValuePair() { }
    
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this._key = key;
            this.value = value;
        }
    }
}