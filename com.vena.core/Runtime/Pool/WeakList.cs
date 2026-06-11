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
    /// A list that can be reused.
    /// The list is created when the first element is added, and is cleared when the Clear method is called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct WeakList<T> : IDisposable
    {
        private _List _object;

        private int _token;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(WeakList<T> obj)
        {
            return obj._object != null && PoolManager.GetToken<_List, int>(obj._object) == obj._token;
        }

        public void Dispose()
        {
            if (TryGetObject(out var list))
            {
                _object = default;
                _token = 0;
                PoolManager.Return<_List, int>(list);
            }
        }

        private bool TryGetObject(out _List list)
        {
            if (_object != null && PoolManager.GetToken<_List, int>(_object) == _token)
            {
                list = _object;
                return true;
            }

            _object = default;
            _token = 0;
            list = default;
            return false;
        }

        private _List GetOrRentObject(int capacity = 0)
        {
            if (TryGetObject(out var list))
            {
                return list;
            }

            list = PoolManager.Rent<_List, int>(capacity);
            _object = list;
            _token = PoolManager.GetToken<_List, int>(list);
            return list;
        }

        public List<T> List
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetOrRentObject().list;
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetObject(out var list))
                {
                    int count = list.list.Count;
                    if (count == 0)
                    {
                        _object = default;
                        _token = 0;
                        PoolManager.Return<_List, int>(list);
                    }

                    return count;
                }

                return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            if (TryGetObject(out var list))
            {
                return list.list.ToArray();
            }

            return Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (TryGetObject(out var list))
            {
                _object = default;
                _token = 0;
                PoolManager.Return<_List, int>(list);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            GetOrRentObject().list.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T value)
        {
            if (TryGetObject(out var list))
            {
                if (list.list.Remove(value))
                {
                    int count = list.list.Count;
                    if (count == 0)
                    {
                        _object = default;
                        _token = 0;
                        PoolManager.Return<_List, int>(list);
                    }

                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index)
        {
            if (TryGetObject(out var list))
            {
                int count = list.list.Count;

                if (index < 0 || index >= count)
                    return false;

                list.list.RemoveAt(index);

                count = list.list.Count;
                if (count == 0)
                {
                    _object = default;
                    _token = 0;
                    PoolManager.Return<_List, int>(list);
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T value)
        {
            if (TryGetObject(out var list))
            {
                return list.list.Contains(value);
            }

            return false;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (TryGetObject(out var list))
                {
                    return list.list[index];
                }

                throw new InvalidOperationException();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (TryGetObject(out var list))
                {
                    list.list[index] = value;
                    return;
                }

                throw new InvalidOperationException();
            }
        }

        public int FindIndex(Predicate<T> match)
        {
            if (TryGetObject(out var list))
            {
                return list.list.FindIndex(match);
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T>.Enumerator GetEnumerator()
        {
            if (!TryGetObject(out var list))
            {
                return _List.Default.list.GetEnumerator();
            }

            return list.list.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            if (TryGetObject(out var list))
            {
                list.list.Sort();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparison<T> comparison)
        {
            if (TryGetObject(out var list))
            {
                list.list.Sort(comparison);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(IComparer<T> comparer)
        {
            if (TryGetObject(out var list))
            {
                list.list.Sort(comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (TryGetObject(out var list))
            {
                list.list.Sort(index, count, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            if (TryGetObject(out var list))
            {
                list.list.Reverse();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count)
        {
            if (TryGetObject(out var list))
            {
                list.list.Reverse(index, count);
            }
        }

        sealed class _List : IPoolable<int>
        {
            public static readonly _List Default = new _List();

            public readonly List<T> list = new List<T>();

            void IPoolable<int>.Construct(in int capacity)
            {
                list.Capacity = System.Math.Max(capacity, list.Capacity);
            }

            void IPoolable<int>.Deconstruct()
            {
                list.Clear();
            }
        }
    }
}
