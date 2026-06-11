// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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

        private int _token;

        public WeakMap(int capacity = 0)
        {
            _capacity = capacity;

            _object = default;

            _token = 0;
        }

        public static implicit operator bool(WeakMap<TKey, TValue> obj)
        {
            return obj._object != null && PoolManager.GetToken<Map, int>(obj._object) == obj._token;
        }

        public void Dispose()
        {
            if (TryGetObject(out var map))
            {
                _object = default;
                _token = 0;
                PoolManager.Return<Map, int>(map);
            }
        }

        private bool TryGetObject(out Map map)
        {
            if (_object != null && PoolManager.GetToken<Map, int>(_object) == _token)
            {
                map = _object;
                return true;
            }

            _object = default;
            _token = 0;
            map = default;
            return false;
        }

        private Map GetOrRentObject()
        {
            if (TryGetObject(out var map))
            {
                return map;
            }

            map = PoolManager.Rent<Map, int>(_capacity);
            _object = map;
            _token = PoolManager.GetToken<Map, int>(map);
            return map;
        }

        public Dictionary<TKey, TValue> Dictionary
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetOrRentObject().dictionary;
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!TryGetObject(out var map))
                {
                    return Map.Default.Values;
                }

                return map.Values;
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!TryGetObject(out var map))
                {
                    return Map.Default.Keys;
                }

                return map.Keys;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (TryGetObject(out var map))
            {
                _object = default;
                _token = 0;
                PoolManager.Return<Map, int>(map);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            GetOrRentObject().Add(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            return TryGetObject(out var map) && map.TryGetValue(key, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            if (TryGetObject(out var map) && map.Remove(key))
            {
                if (map.Count == 0)
                {
                    _object = default;
                    _token = 0;
                    PoolManager.Return<Map, int>(map);
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            return TryGetObject(out var map) && map.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(TValue value)
        {
            return TryGetObject(out var map) && map.ContainsValue(value);
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetObject(out var map))
                {
                    int count = map.Count;
                    if (count == 0)
                    {
                        _object = default;
                        _token = 0;
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
                if (TryGetObject(out var map))
                {
                    return map[key];
                }

                throw new KeyNotFoundException();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                GetOrRentObject()[key] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            if (!TryGetObject(out var map))
            {
                return Map.Default.GetEnumerator();
            }

            return map.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value)
        {
            return GetOrRentObject().TryAdd(key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value)
        {
            if (TryGetObject(out var map) && map.TryRemove(key, out value))
            {
                if (map.Count == 0)
                {
                    _object = default;
                    _token = 0;
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
            return GetOrRentObject().TryUpdate(key, newValue, comparisonValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value, TValue defaultValue)
        {
            value = default;

            return TryGetObject(out var map) && map.TryGetValue(key, out value, defaultValue);
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
