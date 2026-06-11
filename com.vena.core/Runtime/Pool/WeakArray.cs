// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

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

        private int _token;

        public WeakArray(int capacity)
        {
            _capacity = capacity;

            _array = default;

            _token = 0;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => TryGetArray(out var array) ? array.array.Length : 0;
        }

        public void Clear()
        {
            if (TryGetArray(out var array))
            {
                _array = default;
                _token = 0;

                PoolManager.Return<Array, int>(array);
            }
        }

        private bool TryGetArray(out Array array)
        {
            if (_array != null && PoolManager.GetToken<Array, int>(_array) == _token)
            {
                array = _array;
                return true;
            }

            _array = default;
            _token = 0;
            array = default;
            return false;
        }

        private Array GetOrRentArray()
        {
            if (TryGetArray(out var array))
            {
                return array;
            }

            array = PoolManager.Rent<Array, int>(_capacity);
            _array = array;
            _token = PoolManager.GetToken<Array, int>(array);
            return array;
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

                if (TryGetArray(out var array))
                {
                    return array.array[index];
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

                GetOrRentArray().array[index] = value;
            }
        }

        public T[] ToArray()
        {
            if (TryGetArray(out var array))
            {
                T[] toArray = new T[array.array.Length];
                System.Array.Copy(array.array, toArray, array.array.Length);
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
            if (TryGetArray(out var array))
            {
                length = System.Math.Min(arrayIndex + length, array.array.Length);

                for (int i = arrayIndex; i < length; i++)
                {
                    destination.Add(array.array[i]);
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
            if (source._array != null && PoolManager.GetToken<Array, int>(source._array) == source._token)
            {
                if (destinationIndex < 0 || length < 0 || destinationIndex + length > destination._capacity)
                {
                    throw new IndexOutOfRangeException($"destination capacity is not enough");
                }

                if (destination.TryGetArray(out var destinationArray))
                {
                    var sourceArray = source._array;
                    System.Array.Copy(sourceArray.array, sourceIndex, destinationArray.array, destinationIndex, length);
                    return;
                }

                destinationArray = destination.GetOrRentArray();
                System.Array.Copy(source._array.array, sourceIndex, destinationArray.array, destinationIndex, length);
                return;
            }

            throw new IndexOutOfRangeException($"source array is null");
        }

        public void Dispose()
        {
            if (TryGetArray(out var array))
            {
                _array = default;
                _token = 0;

                PoolManager.Return<Array, int>(array);
            }
        }

        public Enumerator GetEnumerator()
        {
            if (TryGetArray(out var array))
            {
                return new Enumerator(array, 0, array.array.Length);
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
