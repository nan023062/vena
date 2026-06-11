// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Vena
{
    /// <summary>
    /// A hash set that can be reused.
    /// The hash set is created when the first element is added, and is cleared when the Clear method is called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct WeakHashSet<T> : IDisposable
    {
        private Set _object;

        private int _token;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(WeakHashSet<T> obj)
        {
            return obj._object != null && PoolManager.GetToken<Set>(obj._object) == obj._token;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetObject(out var set))
                {
                    int count = set.hashSet.Count;
                    if (count == 0)
                    {
                        PoolManager.Return(set);
                        _object = default;
                        _token = 0;
                    }

                    return count;
                }

                return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (TryGetObject(out var set))
            {
                PoolManager.Return(set);
                _object = default;
                _token = 0;
            }
        }

        private bool TryGetObject(out Set set)
        {
            if (_object != null && PoolManager.GetToken<Set>(_object) == _token)
            {
                set = _object;
                return true;
            }

            _object = default;
            _token = 0;
            set = default;
            return false;
        }

        private Set GetOrRentObject()
        {
            if (TryGetObject(out var set))
            {
                return set;
            }

            set = PoolManager.Rent<Set>();
            _object = set;
            _token = PoolManager.GetToken<Set>(set);
            return set;
        }

        public HashSet<T> HashSet
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetOrRentObject().hashSet;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (TryGetObject(out var set))
            {
                PoolManager.Return(set);
                _object = default;
                _token = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            return GetOrRentObject().hashSet.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            if (TryGetObject(out var set) && set.hashSet.Remove(item))
            {
                int count = set.hashSet.Count;
                if (count == 0)
                {
                    PoolManager.Return(set);
                    _object = default;
                    _token = 0;
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return TryGetObject(out var set) && set.hashSet.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (!TryGetObject(out var set))
            {
                return Array.Empty<T>();
            }

            return set.hashSet.ToArray();
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            if (!TryGetObject(out var set))
            {
                return Set.Default.hashSet.GetEnumerator();
            }

            return set.hashSet.GetEnumerator();
        }

        sealed class Set : IPoolable
        {
            public static readonly Set Default = new Set();

            public readonly HashSet<T> hashSet = new HashSet<T>();

            void IPoolable.Deconstruct()
            {
                hashSet.Clear();
            }

            void IPoolable.Construct()
            {
                hashSet.Clear();
            }
        }
    }
}
