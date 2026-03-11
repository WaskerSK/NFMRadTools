using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Configuration
{
    public sealed class ConfigurableEntry : ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, System.Collections.IEnumerable, IDictionary<string, string>, IReadOnlyCollection<KeyValuePair<string, string>>, IReadOnlyDictionary<string, string>, IDeserializationCallback, ISerializable
    {
        private Dictionary<string, string> _dictionary;

        public ConfigurableEntry()
        {
            _dictionary = new Dictionary<string, string>();
        }

        public ConfigurableEntry(int capacity)
        {
            _dictionary = new Dictionary<string, string>(capacity);
        }

        public string this[string key] { get => _dictionary[key]; set => _dictionary[key] = value; }

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<string> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_dictionary).IsReadOnly;

        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => Keys;

        IEnumerable<string> IReadOnlyDictionary<string, string>.Values => Values;

        public void Add(string key, string value)
        {
            _dictionary.Add(key, value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _dictionary.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public Dictionary<string, string>.Enumerator GetEnumerator() => _dictionary.GetEnumerator();

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>)_dictionary).CopyTo(array, arrayIndex);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_dictionary).GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
#pragma warning disable
            ((ISerializable)_dictionary).GetObjectData(info, context);
#pragma warning restore
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            ((IDeserializationCallback)_dictionary).OnDeserialization(sender);
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            return ((ICollection<KeyValuePair<string, string>>)_dictionary).Remove(item);
        }
    }
}
