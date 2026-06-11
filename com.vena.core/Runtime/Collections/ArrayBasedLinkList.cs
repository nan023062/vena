// -----------------------------------------------------------------------------
// Vena Core
// Core primitives for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vena
{
    /// <summary>
    ///  An array-based linked list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ArrayBasedLinkList<T> : IEnumerable<T>
    {
        public const int Invalid = -1;
        
        private Node[] _array;
        
        private int _head;
        
        private int _tail;
        
        private int _count;
        
        private int _freeHead;
        
        public int Head => _head;
        
        public int Tail => _tail;
        
        public int Count => _count;
        
        private int _version;
        
        public ArrayBasedLinkList(int capacity = 4)
        {
            _array = new Node[capacity < 4 ? 4 : capacity];
            _head = _tail = Invalid;
            _freeHead = 0;
            _count = 0;
            _version = 0;
            
            for (int i = 0; i < _array.Length; i++)
            {
                ref var node = ref _array[i];
                node.Index = i;
                node.Value = default;
                node.Prev = i == 0 ? Invalid : i - 1;
                node.Next = i == _array.Length - 1 ? Invalid : i + 1;
            }
        }
        
        public int AddLast(T item)
        {
            if (_freeHead == Invalid)
            {
                Resize(_array.Length * 2);
            }
            
            int index = _freeHead;
            ref var newNode = ref _array[index];
            int nextFree = newNode.Next;
            newNode.Value = item;
            newNode.Index = index;
            newNode.Prev = _tail;
            newNode.Next = Invalid;
            newNode.InUse = true;
            
            if (_count == 0)
            {
                _head = _tail = index;
            }
            else
            {
                _array[_tail].Next = index;
                _tail = index;
            }
            
            _freeHead = nextFree;
            _count++;
            _version = (_version + 1) % (int.MaxValue - 2);
            return index;
        }
        
        public int AddFirst(T item)
        {
            if (_freeHead == Invalid)
            {
                Resize(_array.Length * 2);
            }
            
            int index = _freeHead;
            ref var newNode = ref _array[index];
            int nextFree = newNode.Next;
            newNode.Value = item;
            newNode.Index = index;
            newNode.Prev = Invalid;
            newNode.Next = _head;
            newNode.InUse = true;
            
            if (_count == 0)
            {
                _head = _tail = index;
            }
            else
            {
                _array[_head].Prev = index;
                _head = index;
            }
            
            _freeHead = nextFree;
            _count++;
            _version = (_version + 1) % (int.MaxValue - 2);
            return index;
        }
        
        public ref T GetValue(int index)
        {
            if(index < 0 || index >= _array.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            
            ref var node = ref _array[index];
            if (!node.InUse)
                throw new System.InvalidOperationException("Node is not in the list.");

            return ref node.Value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueUnchecked(int index)
        {
            return ref _array[index].Value;
        }
        
        public ref T GetValueAndNext(int index, out int next)
        {
            if(index < 0 || index >= _array.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            
            ref var node = ref _array[index];
            if (!node.InUse)
                throw new System.InvalidOperationException("Node is not in the list.");

            next = node.Next;
            return ref node.Value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetValueAndNextUnchecked(int index, out int next)
        {
            ref var node = ref _array[index];
            next = node.Next;
            return ref node.Value;
        }
        
        public int GetNext(int index)
        {
            if(index < 0 || index >= _array.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            
            if (!_array[index].InUse)
                throw new System.InvalidOperationException("Node is not in the list.");

            return _array[index].Next;
        }
        
        public int GetPrev(int index)
        {
            if(index < 0 || index >= _array.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            
            if (!_array[index].InUse)
                throw new System.InvalidOperationException("Node is not in the list.");

            return _array[index].Prev;
        }
        
        public void Remove(int index)
        {
            if(index < 0 || index >= _array.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));
            
            ref var node = ref _array[index];
            if (!node.InUse)
                throw new System.InvalidOperationException("Node is not in the list.");

            if (node.Prev != Invalid)
            {
                _array[node.Prev].Next = node.Next;
            }
            else
            {
                _head = node.Next;
            }
            
            if (node.Next != Invalid)
            {
                _array[node.Next].Prev = node.Prev;
            }
            else
            {
                _tail = node.Prev;
            }
            
            node.Prev = Invalid;
            node.Value = default;
            node.Next = _freeHead;
            node.InUse = false;
            _freeHead = index;
            _count--;
            
            if (_count == 0)
            {
                _head = _tail = Invalid;
            }
            _version = (_version + 1) % (int.MaxValue - 2);
        }

        public void Clear(bool reset = false)
        {
            if (reset)
            {
                _array = new Node[4];
            }

            for (int i = 0; i < _array.Length; i++)
            {
                ref var node = ref _array[i];
                node.Prev = i == 0 ? Invalid : i - 1;
                node.Next = i == _array.Length - 1 ? Invalid : i + 1;
                node.Value = default;
                node.Index = i;
                node.InUse = false;
            }
            
            _head = _tail = Invalid;
            _freeHead = 0;
            _count = 0;
            _version = (_version + 1) % (int.MaxValue - 2);
        }
        
        private void Resize(int capacity)
        {
            int oldLength = _array.Length;
            Node[] newArray = new Node[capacity];
            System.Array.Copy( _array, 0, newArray, 0, oldLength);
            _array = newArray;
            
            for (int i = oldLength; i < _array.Length; i++)
            {
                ref var node = ref _array[i];
                node.Index = i;
                node.Value = default;
                node.Prev = Invalid;
                node.Next = i == _array.Length - 1 ? Invalid : i + 1;
                node.InUse = false;
            }
            _freeHead = oldLength;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        struct Node
        {
            public T Value;
            public int Index;
            public int Prev;
            public int Next;
            public bool InUse;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly int _version;
            
            private readonly ArrayBasedLinkList<T> _list;
            
            private int _current;

            private int _next;

            public Enumerator(ArrayBasedLinkList<T> linkList)
            {
                _version = linkList._version;
                _list = linkList;
                _current = Invalid;
                _next = linkList._head;
            }

            public object Current
            {
                get
                {
                    if (_current == Invalid)
                        throw new System.InvalidOperationException("Enumeration has either not started or has already finished.");

                    return _list._array[_current].Value;
                }
            }
            
            T IEnumerator<T>.Current
            {
                get
                {
                    if (_current == Invalid)
                        throw new System.InvalidOperationException("Enumeration has either not started or has already finished.");

                    return _list._array[_current].Value;
                }
            }

            public bool MoveNext()
            {
                if(_version != _list._version)
                    throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                
                if (_next == Invalid)
                {
                    _current = Invalid;
                    return false;
                }
                
                _current = _next;
                _next = _list._array[_current].Next;
                return true;
            }

            public void Reset()
            {
                _current = Invalid;
                _next = _list._head;
            }
            
            public void Dispose()
            {
                _current = Invalid;
                _next = Invalid;
            }
        }
    }
}
