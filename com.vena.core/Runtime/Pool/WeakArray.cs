using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena
{
    public struct WeakArray<T> : IDisposable
    {
        private readonly int _capacity;

        private Array _array;

        public WeakArray(int capacity)
        {
            _capacity = capacity;

            _array = default;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => null != _array ? _array.array.Length : 0;
        }

        public void Clear()
        {
            if (null != _array)
            {
                var array = _array;

                _array = default;

                PoolManager.Return<Array, int>(array);
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _capacity)
                {
                    throw new IndexOutOfRangeException();
                }

                if (_array != null)
                {
                    return _array.array[index];
                }

                return default;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= _capacity)
                {
                    throw new IndexOutOfRangeException();
                }

                _array ??= PoolManager.Rent<Array, int>(_capacity);

                _array.array[index] = value;
            }
        }

        public T[] ToArray()
        {
            if (_array != null)
            {
                T[] toArray = new T[_array.array.Length];
                System.Array.Copy(_array.array, toArray, _array.array.Length);
                return toArray;
            }

            return System.Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(List<T> destination)
        {
            CopyTo(0, destination, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int arrayIndex, List<T> destination, int length)
        {
            if (_array != null)
            {
                length = System.Math.Min(arrayIndex + length, _array.array.Length);

                for (int i = arrayIndex; i < length; i++)
                {
                    destination.Add(_array.array[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(in WeakArray<T> source, ref WeakArray<T> destination, int length)
        {
            Copy(source, 0, ref destination, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(in WeakArray<T> source, int sourceIndex, ref WeakArray<T> destination,
            int destinationIndex, int length)
        {
            if (source._array != null)
            {
                if (destination._array != null)
                {
                    var sourceArray = source._array;
                    var destinationArray = destination._array;
                    System.Array.Copy(sourceArray.array, sourceIndex, destinationArray.array, destinationIndex, length);
                    return;
                }

                throw new IndexOutOfRangeException($"destination array is null");
            }

            throw new IndexOutOfRangeException($"source array is null");
        }

        public void Dispose()
        {
            if (null != _array)
            {
                var array = _array;

                _array = default;

                PoolManager.Return<Array, int>(array);
            }
        }

        public Enumerator GetEnumerator()
        {
            if (null != _array)
            {
                return new Enumerator(_array, 0, _array.array.Length);
            }

            return new Enumerator(Array.Empty, 0, 0);
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly Array array;
            private readonly int endIndex;
            private readonly int startIndex;
            private int index;
            private bool _complete;

            internal Enumerator(Array array, int index, int count)
            {
                this.array = array;

                if (array._lock)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }

                this.array._lock = true;
                startIndex = index - 1;
                endIndex = index + count;
                this.index = startIndex;
                _complete = false;
            }

            public bool MoveNext()
            {
                if (_complete)
                {
                    index = endIndex;
                    return false;
                }

                ++index;
                _complete = index >= endIndex;
                return !_complete;
            }

            public void Reset()
            {
                index = startIndex;
                _complete = false;
            }

            object IEnumerator.Current => ((IEnumerator<T>)this).Current;

            T IEnumerator<T>.Current
            {
                get
                {
                    if (index < startIndex)
                        throw new InvalidOperationException("InvalidOperation_EnumNotStarted");
                    if (_complete)
                        throw new InvalidOperationException("InvalidOperation_EnumEnded");
                    return array.array[index];
                }
            }

            public void Dispose()
            {
                // TODO release managed resources here
                array._lock = false;
            }
        }

        internal class Array : IPoolable<int>
        {
            public static readonly Array Empty = new Array();

            public T[] array = System.Array.Empty<T>();
            public bool _lock;

            public void Construct(in int capacity)
            {
                if (capacity >= 65536)
                {
                    throw new ArgumentOutOfRangeException("capacity is too large");
                }

                // make capacity power of 2
                int lenth = System.Math.Max(1, capacity);
                int size = 1;
                while (size < lenth) size <<= 1;

                // rent array
                array = new T[size];
                _lock = false;
            }

            public void Deconstruct()
            {
                _lock = false;
                // clear array
                System.Array.Clear(array, 0, array.Length);
            }
        }
    }
}