//****************************************************************************
// File: PriorityQueue.cs
// Author: Li Nan
// Date: 2023-08-24 12:00
// Version: 1.0
//****************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Vena
{
    /// <summary>
    /// 优先队列
    /// </summary>
    /// <typeparam name="T"> IComparable </typeparam>
    public sealed class PriorityQueue<T> : IDisposable where T : IComparable<T>
    {
        public int Count { private set; get; }
    
        private T[] _array;
    
        public PriorityQueue(int capacity = 16)
        {
            _array = new T[capacity + 1];
            Count = 0;
        }
    
        public void Clear()
        {
            if (Count > 0)
            {
                for (int i = 1; i <= Count; i++)
                {
                    _array[i] = default;
                }
                
                Count = 0;
            }
        }
    
        public void Dispose()
        {
            _array = null;
            
            Count = 0;
        }
    
        public ref readonly T Peek()
        {
            return ref _array[1];
        }
    
        public void Enqueue(T element)
        {
            if (Count == _array.Length - 1)
            {
                Array.Resize(ref _array, _array.Length * 2);
            }
    
            _array[++Count] = element;
    
            int i = Count;
            
            while (i > 1 && _array[i].CompareTo(_array[i >> 1]) < 0)
            {
                Swap(i, i >> 1);
                
                i >>= 1;
            }
        }
    
        public T Dequeue()
        {
            T element = _array[1];
            
            _array[1] = default;
            
            Swap(1, Count--);
    
            int i = 1;
            
            while ((i << 1) <= Count)
            {
                int k = i << 1;
                
                if (k < Count)
                {
                    if (_array[k + 1].CompareTo(_array[k]) < 0)
                    {
                        k += 1;
                    }
                }

                if (_array[k].CompareTo(_array[i]) >= 0)
                {
                    break;
                }
    
                Swap(k, i);
                
                i = k;
            }
    
            return element;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int a, int b)
        {
            (_array[a], _array[b]) = (_array[b], _array[a]);
        }
    }
}

