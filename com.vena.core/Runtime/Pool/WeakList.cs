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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(WeakList<T> obj)
        {
            return obj._object != null;
        }

        public void Dispose()
        {
            if (_object != null)
            {
                var list = _object;
                _object = default;
                PoolManager.Return<_List, int>(list);
            }
        }

        public List<T> List
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                _object ??= PoolManager.Rent<_List, int>(0);

                return _object.list;
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_object != null)
                {
                    int count = _object.list.Count;
                    if (count == 0)
                    {
                        var list = _object;
                        _object = default;
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
            if (_object != null)
            {
                return _object.list.ToArray();
            }

            return Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_object != null)
            {
                var list = _object;
                _object = default;
                PoolManager.Return<_List, int>(list);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            _object ??= PoolManager.Rent<_List, int>(0);

            _object.list.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T value)
        {
            if (_object != null)
            {
                if (_object.list.Remove(value))
                {
                    int count = _object.list.Count;
                    if (count == 0)
                    {
                        var list = _object;
                        _object = default;
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
            if (_object != null)
            {
                int count = _object.list.Count;

                if (index < 0 || index >= count)
                    return false;

                _object.list.RemoveAt(index);

                count = _object.list.Count;
                if (count == 0)
                {
                    var list = _object;
                    _object = default;
                    PoolManager.Return<_List, int>(list);
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T value)
        {
            if (_object != null)
            {
                return _object.list.Contains(value);
            }

            return false;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_object != null)
                {
                    return _object.list[index];
                }

                throw new InvalidOperationException();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_object != null)
                {
                    _object.list[index] = value;
                    return;
                }

                throw new InvalidOperationException();
            }
        }

        public int FindIndex(Predicate<T> match)
        {
            if (_object != null)
            {
                return _object.list.FindIndex(match);
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T>.Enumerator GetEnumerator()
        {
            if (null == _object)
            {
                return _List.Default.list.GetEnumerator();
            }

            return _object.list.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            if (_object != null)
            {
                _object.list.Sort();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(Comparison<T> comparison)
        {
            if (_object != null)
            {
                _object.list.Sort(comparison);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(IComparer<T> comparer)
        {
            if (_object != null)
            {
                _object.list.Sort(comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            if (_object != null)
            {
                _object.list.Sort(index, count, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            if (_object != null)
            {
                _object.list.Reverse();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count)
        {
            if (_object != null)
            {
                _object.list.Reverse(index, count);
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