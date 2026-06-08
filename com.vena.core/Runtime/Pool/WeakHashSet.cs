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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(WeakHashSet<T> obj)
        {
            return obj._object != null;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_object != null)
                {
                    int count = _object.hashSet.Count;
                    if (count == 0)
                    {
                        var set = _object;
                        PoolManager.Return(set);
                        _object = default;
                    }

                    return count;
                }

                return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_object != null)
            {
                var set = _object;
                PoolManager.Return(set);
                _object = default;
            }
        }

        public HashSet<T> HashSet
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _object ??= PoolManager.Rent<Set>();

                return _object.hashSet;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_object != null)
            {
                var set = _object;
                PoolManager.Return(set);
                _object = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item)
        {
            _object ??= PoolManager.Rent<Set>();

            return _object.hashSet.Add(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            if (_object != null && _object.hashSet.Remove(item))
            {
                int count = _object.hashSet.Count;
                if (count == 0)
                {
                    var set = _object;
                    PoolManager.Return(set);
                    _object = default;
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return _object != null && _object.hashSet.Contains(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (null == _object)
            {
                return Array.Empty<T>();
            }

            return _object.hashSet.ToArray();
        }

        public HashSet<T>.Enumerator GetEnumerator()
        {
            if (null == _object)
            {
                return Set.Default.hashSet.GetEnumerator();
            }

            return _object.hashSet.GetEnumerator();
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