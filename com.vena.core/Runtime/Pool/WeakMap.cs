using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena
{
    /// <summary>
    /// A dictionary that can be reused.
    /// The dictionary is created when the first element is added, and is cleared when the Clear method is called.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public struct WeakMap<TKey, TValue> : IDisposable
    {
        private readonly int _capacity;

        private Map _object;

        public WeakMap(int capacity = 0)
        {
            _capacity = capacity;

            _object = default;
        }

        public static implicit operator bool(WeakMap<TKey, TValue> obj)
        {
            return obj._object != null;
        }

        public void Dispose()
        {
            if (_object != null)
            {
                var map = _object;
                _object = default;
                PoolManager.Return<Map, int>(map);
            }
        }

        public Dictionary<TKey, TValue> Dictionary
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _object ??= PoolManager.Rent<Map, int>(_capacity);

                return _object.dictionary;
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (null == _object)
                {
                    return Map.Default.Values;
                }

                return _object.Values;
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (null == _object)
                {
                    return Map.Default.Keys;
                }

                return _object.Keys;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (null != _object)
            {
                var map = _object;
                _object = default;
                PoolManager.Return<Map, int>(map);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            _object ??= PoolManager.Rent<Map, int>(_capacity);

            _object.Add(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            return null != _object && _object.TryGetValue(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            if (null != _object && _object.Remove(key))
            {
                if (_object.Count == 0)
                {
                    var map = _object;
                    _object = default;
                    PoolManager.Return<Map, int>(map);
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            return null != _object && _object.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(TValue value)
        {
            return null != _object && _object.ContainsValue(value);
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (null != _object)
                {
                    int count = _object.Count;
                    if (count == 0)
                    {
                        var map = _object;
                        _object = default;
                        PoolManager.Return<Map, int>(map);
                    }

                    return count;
                }

                return 0;
            }
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (null != _object)
                {
                    return _object[key];
                }

                throw new KeyNotFoundException();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _object ??= PoolManager.Rent<Map, int>(_capacity);

                _object[key] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            if (null == _object)
            {
                return Map.Default.GetEnumerator();
            }

            return _object.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value)
        {
            _object ??= PoolManager.Rent<Map, int>(_capacity);

            return _object.TryAdd(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value)
        {
            if (null != _object && _object.TryRemove(key, out value))
            {
                if (_object.Count == 0)
                {
                    var map = _object;
                    _object = default;
                    PoolManager.Return<Map, int>(map);
                }

                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            _object ??= PoolManager.Rent<Map, int>(_capacity);

            return _object.TryUpdate(key, newValue, comparisonValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value, TValue defaultValue)
        {
            value = default;

            return null != _object && _object.TryGetValue(key, out value, defaultValue);
        }


        #region reusable

        sealed class Map : IPoolable<int>
        {
            public static readonly Map Default = new Map();

            public readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            void IPoolable<int>.Construct(in int capacity)
            {
                dictionary.Clear();
            }

            void IPoolable<int>.Deconstruct()
            {
                dictionary.Clear();
            }

            public Dictionary<TKey, TValue>.ValueCollection Values
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => dictionary.Values;
            }

            public Dictionary<TKey, TValue>.KeyCollection Keys
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => dictionary.Keys;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(TKey key, TValue value)
            {
                dictionary.Add(key, value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(TKey key, out TValue value)
            {
                return dictionary.TryGetValue(key, out value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(TKey key)
            {
                return dictionary.Remove(key);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(TKey key)
            {
                return dictionary.ContainsKey(key);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsValue(TValue value)
            {
                return dictionary.ContainsValue(value);
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => dictionary.Count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
            {
                return dictionary.GetEnumerator();
            }

            public TValue this[TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => dictionary[key];

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => dictionary[key] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAdd(TKey key, TValue value)
            {
                if (dictionary.ContainsKey(key))
                {
                    return false;
                }

                dictionary.Add(key, value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryRemove(TKey key, out TValue value)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    dictionary.Remove(key);
                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
            {
                if (dictionary.TryGetValue(key, out var value) &&
                    EqualityComparer<TValue>.Default.Equals(value, comparisonValue))
                {
                    dictionary[key] = newValue;
                    return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(TKey key, out TValue value, TValue defaultValue)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    return true;
                }

                value = defaultValue;
                return false;
            }
        }

        #endregion
    }
}