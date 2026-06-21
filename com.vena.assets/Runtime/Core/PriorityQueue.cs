// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;

namespace Vena.Assets
{
    /// <summary>
    /// 优先队列
    /// </summary>
    public sealed class PriorityQueue<T> : IDisposable where T : IComparable<T>
    {
        public int Count { private set; get; }
        
        private T[] _array;
        
        public PriorityQueue(int capacity = 20)
        {
            _array = new T[capacity + 1];
            Count = 0;
        }
        
        public void Dispose()
        {
            _array = null;
            Count = 0;
        }
        
        public T Peek()
        {
            return _array[1];
        }
        
        public void Enqueue(T element)
        {
            if (Count == _array.Length - 1)
            {
                Array.Resize(ref _array, _array.Length * 2);
            }
    
            _array[++Count] = element;
    
            int index = Count;
            while (index > 1 && _array[index].CompareTo(_array[index >> 1]) == 1)
            {
                Swap(index, index >> 1);
                index >>= 1;
            }
        }
        
        public T Dequeue()
        {
            T element = _array[1];
            Swap(1, Count--);
    
            int index = 1;
            while ((index << 1) <= Count)
            {
                int k = index << 1;
                if (_array[k + 1].CompareTo(_array[k]) == 1)
                    k += 1;
    
                if (_array[k].CompareTo(_array[index]) != 1)
                    break;
                
                Swap(k, index);
                index = k;
            }
    
            return element;
        }
        
        private void Swap(int a, int b)
        {
            (_array[a], _array[b]) = (_array[b], _array[a]);
        }
    }
}

