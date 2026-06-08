using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena
{
    public class FastList<T>
    {
        private T[] _array;
        private int _size;

        public FastList(int capacity)
        {
            this._array = new T[capacity];
            this._size = 0;
        }

        public int Capacity => this._array?.Length ?? 0;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                if (index < 0 || index >= this._size)
                    throw new IndexOutOfRangeException();
#endif
                return ref this._array[index];
            }
        }

        public ref readonly T GetUnchecked(int index)
        {
            return ref this._array[index];
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this._size;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T elem)
        {
            int index = this._size;
            this._size += 1;
            if (this._array.Length < this._size)
            {
                Array.Resize(ref this._array, this._array.Length * 2);
            }

            this._array[index] = elem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast()
        {
            if (this._size == 0) return;
            this._array[--this._size] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastRemoveAt(int index)
        {
            this._array[index] = this._array[--this._size];
            this._array[this._size] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort()
        {
            if (this._size == 0) return;
            Array.Sort(_array, 0, _size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(IComparer<T> comparer)
        {
            if (this._size == 0) return;
            Array.Sort(_array, 0, _size, comparer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T other, Func<T, T, bool> comparer)
        {
            for (var i = 0; i < Count; ++i)
            {
                if (comparer(this[i], other))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (this._size > 0)
            {
                Array.Clear(this._array, 0, this._size);
            }

            this._size = 0;
        }

        public void Resize(int newSize)
        {
            if (this._size > newSize)
            {
                Array.Clear(this._array, newSize, this._size - newSize);
                this._size = newSize;
            }
            else
            {
                for (int i = this._size; i < newSize; i++)
                {
                    Add(default);
                }
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }
    }
}